using CurrencyConverter.Application.Common.Abstractions;
using CurrencyConverter.Domain.Common;
using CurrencyConverter.Domain.ExchangeRates;
using CurrencyConverter.Providers.ExchangeRates.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http.Json;
using System.Text.Json;

namespace CurrencyConverter.Providers.ExchangeRates
{
    public class FrankfurterCurrencyProvider : ICurrencyProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FrankfurterCurrencyProvider> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
        private readonly HashSet<string> _restrictedCurrencies = new() { "TRY", "PLN", "THB", "MXN" };

        private const string CacheKeyPrefix = "FrankfurterApi:";
        private const string ApiSource = "Frankfurter API";

        public FrankfurterCurrencyProvider(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<FrankfurterCurrencyProvider> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configure retry policy with exponential backoff
            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    3, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Request failed with {StatusCode}. Waiting {TimeSpan} before retry. Retry attempt {RetryCount}",
                            outcome.Result.StatusCode, timespan, retryCount);
                    });

            // Configure circuit breaker
            _circuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    5, // Number of consecutive failures before breaking the circuit
                    TimeSpan.FromMinutes(1), // Duration the circuit stays open before trying again
                    onBreak: (outcome, timespan) =>
                    {
                        _logger.LogError(
                            "Circuit breaker tripped. Service will be unavailable for {TimeSpan}",
                            timespan);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset. Service available again.");
                    });
        }

        public async Task<ExchangeRateRecord> GetLatestRatesAsync(string baseCurrency)
        {
            ValidateCurrency(baseCurrency);

            string cacheKey = $"{CacheKeyPrefix}Latest:{baseCurrency}";
            
            if (_cache.TryGetValue(cacheKey, out ExchangeRateRecord cachedRates))
            {
                _logger.LogInformation("Retrieved latest rates for {BaseCurrency} from cache", baseCurrency);
                return cachedRates;
            }

            string endpoint = $"latest?base={baseCurrency}";
            var response = await ExecuteWithResilienceAsync(endpoint);

            var exchangeRate = MapToExchangeRate(response, baseCurrency);
            
            // Cache for 1 hour
            _cache.Set(cacheKey, exchangeRate, TimeSpan.FromHours(1));
            
            return exchangeRate;
        }

        public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            ValidateCurrency(fromCurrency);
            ValidateCurrency(toCurrency);
                
            string cacheKey = $"{CacheKeyPrefix}Convert:{amount}:{fromCurrency}:{toCurrency}";
            
            if (_cache.TryGetValue(cacheKey, out decimal cachedAmount))
            {
                _logger.LogInformation("Retrieved conversion for {Amount} {FromCurrency} to {ToCurrency} from cache", 
                    amount, fromCurrency, toCurrency);
                return cachedAmount;
            }

            // Updated to match Frankfurter API format
            string endpoint = $"latest?base={fromCurrency}&symbols={toCurrency}";
            var response = await ExecuteWithResilienceAsync(endpoint);

            decimal convertedAmount = amount;
            if (response.Rates.TryGetValue(toCurrency, out decimal rate))
            {
                convertedAmount = amount * rate;
            }
            
            // Cache for 1 hour
            _cache.Set(cacheKey, convertedAmount, TimeSpan.FromHours(1));
            
            return convertedAmount;
        }

        public async Task<(IEnumerable<ExchangeRateRecord> Rates, int TotalCount)> GetHistoricalRatesAsync(
            string baseCurrency, 
            DateTime startDate, 
            DateTime endDate, 
            int page, 
            int pageSize)
        {
            ValidateCurrency(baseCurrency);

            if (startDate > endDate)
                throw new ArgumentException("Start date must be before or equal to end date");

            if (page < 1)
                throw new ArgumentException("Page must be greater than or equal to 1");

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than or equal to 1");

            string cacheKey = $"{CacheKeyPrefix}Historical:{baseCurrency}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";
            
            if (!_cache.TryGetValue(cacheKey, out List<ExchangeRateRecord> allRates))
            {
                string endpoint = $"{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={baseCurrency}";
                var response = await ExecuteWithResilienceAsync<FrankfurterHistoricalResponse>(endpoint);

                allRates = MapToHistoricalExchangeRates(response, baseCurrency);
                
                // Cache for 24 hours since historical data doesn't change
                _cache.Set(cacheKey, allRates, TimeSpan.FromHours(24));
            }

            int totalCount = allRates.Count;
            var paginatedRates = allRates
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (paginatedRates, totalCount);
        }

        private async Task<FrankfurterApiResponse> ExecuteWithResilienceAsync(string endpoint)
        {
            return await ExecuteWithResilienceAsync<FrankfurterApiResponse>(endpoint);
        }

        private async Task<T> ExecuteWithResilienceAsync<T>(string endpoint)
        {
            _logger.LogInformation("Calling Frankfurter API: {Endpoint}", endpoint);
            
            var response = await _retryPolicy
                .WrapAsync(_circuitBreakerPolicy)
                .ExecuteAsync(() => _httpClient.GetAsync(endpoint));

            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<T>();
            if (result == null)
                throw new InvalidOperationException("Failed to deserialize API response");
            
            return result;
        }

        private ExchangeRateRecord MapToExchangeRate(FrankfurterApiResponse response, string baseCurrency)
        {
            DateTime timestamp = DateTime.Parse(response.Date);
            var exchangeRate = new ExchangeRateRecord(baseCurrency, timestamp, ApiSource);

            foreach (var rate in response.Rates)
            {
                if (!_restrictedCurrencies.Contains(rate.Key))
                {
                    exchangeRate.AddRate(rate.Key, rate.Value);
                }
            }

            return exchangeRate;
        }

        private List<ExchangeRateRecord> MapToHistoricalExchangeRates(FrankfurterHistoricalResponse response, string baseCurrency)
        {
            var result = new List<ExchangeRateRecord>();

            foreach (var dateRates in response.Rates)
            {
                if (DateTime.TryParse(dateRates.Key, out DateTime date))
                {
                    var exchangeRate = new ExchangeRateRecord(baseCurrency, date, ApiSource);

                    foreach (var rate in dateRates.Value)
                    {
                        if (!_restrictedCurrencies.Contains(rate.Key))
                        {
                            exchangeRate.AddRate(rate.Key, rate.Value);
                        }
                    }

                    result.Add(exchangeRate);
                }
            }

            return result.OrderByDescending(r => r.Timestamp).ToList();
        }

        private void ValidateCurrency(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency code cannot be null or empty", nameof(currency));

            if (_restrictedCurrencies.Contains(currency))
                throw new DomainException($"Currency {currency} is restricted");
        }
    }
}
