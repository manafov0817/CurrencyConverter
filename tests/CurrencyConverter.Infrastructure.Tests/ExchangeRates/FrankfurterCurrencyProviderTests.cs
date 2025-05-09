using CurrencyConverter.Domain.Common;
using CurrencyConverter.Domain.ExchangeRates;
using CurrencyConverter.Providers.ExchangeRates;
using CurrencyConverter.Providers.ExchangeRates.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace CurrencyConverter.Infrastructure.Tests.ExchangeRates
{
    public class FrankfurterCurrencyProviderTests
    {
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ILogger<FrankfurterCurrencyProvider>> _loggerMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly FrankfurterCurrencyProvider _provider;
        private readonly string _baseCurrency = "EUR";
        private readonly string _targetCurrency = "USD";
        private object _cacheValue;

        public FrankfurterCurrencyProviderTests()
        {
            // Setup cache mock
            _cacheMock = new Mock<IMemoryCache>();
            var cacheEntryMock = new Mock<ICacheEntry>();
            _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);
            _cacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out _cacheValue)).Returns(false);

            // Setup logger mock
            _loggerMock = new Mock<ILogger<FrankfurterCurrencyProvider>>();

            // Setup HTTP handler mock
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app/")
            };

            // Create provider with mocks
            _provider = new FrankfurterCurrencyProvider(_httpClient, _cacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ReturnsCorrectRates_WhenApiResponseIsValid()
        {
            // Arrange
            var responseData = new FrankfurterApiResponse
            {
                Base = _baseCurrency,
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Rates = new Dictionary<string, decimal>
                {
                    { "USD", 1.1m },
                    { "GBP", 0.9m },
                    { "JPY", 120m }
                }
            };

            SetupHttpResponse(HttpStatusCode.OK, responseData);

            // Act
            var result = await _provider.GetLatestRatesAsync(_baseCurrency);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_baseCurrency, result.BaseCurrency);
            Assert.Equal(3, result.Rates.Count);
            Assert.Equal(1.1m, result.Rates["USD"]);
            Assert.Equal(0.9m, result.Rates["GBP"]);
            Assert.Equal(120m, result.Rates["JPY"]);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ReturnsCachedRates_WhenDataIsInCache()
        {
            // Arrange
            var cachedRates = new ExchangeRateRecord(_baseCurrency, DateTime.UtcNow, "Frankfurter API");
            cachedRates.AddRate("USD", 1.1m);
            cachedRates.AddRate("GBP", 0.9m);

            _cacheValue = cachedRates;
            _cacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out _cacheValue)).Returns(true);

            // Act
            var result = await _provider.GetLatestRatesAsync(_baseCurrency);

            // Assert
            Assert.Same(cachedRates, result);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task ConvertCurrencyAsync_ReturnsCorrectAmount_WhenApiResponseIsValid()
        {
            // Arrange
            decimal amount = 100;
            decimal expectedRate = 1.1m;
            decimal expectedAmount = amount * expectedRate;

            var responseData = new FrankfurterApiResponse
            {
                Base = _baseCurrency,
                Amount = amount,
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Rates = new Dictionary<string, decimal>
                {
                    { _targetCurrency, expectedRate }
                }
            };

            SetupHttpResponse(HttpStatusCode.OK, responseData);

            // Act
            var result = await _provider.ConvertCurrencyAsync(amount, _baseCurrency, _targetCurrency);

            // Assert
            Assert.Equal(expectedAmount, result);
        }

        [Fact]
        public async Task GetHistoricalRatesAsync_ReturnsCorrectRates_WhenApiResponseIsValid()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 3);

            var responseData = new FrankfurterHistoricalResponse
            {
                Base = _baseCurrency,
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd"),
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { "2023-01-01", new Dictionary<string, decimal> { { "USD", 1.1m }, { "GBP", 0.9m } } },
                    { "2023-01-02", new Dictionary<string, decimal> { { "USD", 1.2m }, { "GBP", 0.85m } } },
                    { "2023-01-03", new Dictionary<string, decimal> { { "USD", 1.15m }, { "GBP", 0.88m } } }
                }
            };

            SetupHttpResponse(HttpStatusCode.OK, responseData);

            // Act
            var result = await _provider.GetHistoricalRatesAsync(_baseCurrency, startDate, endDate, 1, 10);

            // Assert
            Assert.NotNull(result.Rates);
            Assert.Equal(3, result.TotalCount);
            Assert.All(result.Rates, rate =>
            {
                Assert.Equal(_baseCurrency, rate.BaseCurrency);
                Assert.Equal(2, rate.Rates.Count);
            });
        }

        [Theory]
        [InlineData("TRY")] // Restricted currency
        [InlineData("PLN")] // Restricted currency
        public async Task GetLatestRatesAsync_ThrowsDomainException_WhenCurrencyIsRestricted(string currency)
        {
            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _provider.GetLatestRatesAsync(currency));
        }

        [Theory]
        [InlineData("")] // Empty string
        [InlineData(null)] // Null
        [InlineData(" ")] // Whitespace
        public async Task GetLatestRatesAsync_ThrowsArgumentException_WhenCurrencyIsInvalid(string currency)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _provider.GetLatestRatesAsync(currency));
        }

        private void SetupHttpResponse<T>(HttpStatusCode statusCode, T content)
        {
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(JsonSerializer.Serialize(content))
                });
        }
    }
}
