using CurrencyConverter.Domain.Common;

namespace CurrencyConverter.Domain.ExchangeRates.Builders
{
    public class ExchangeRateBuilder
    {
        private string _baseCurrency;
        private DateTime _timestamp;
        private string _source;
        private readonly Dictionary<string, decimal> _rates = new Dictionary<string, decimal>();
        private readonly HashSet<string> _restrictedCurrencies = new HashSet<string> { "TRY", "PLN", "THB", "MXN" };

        public ExchangeRateBuilder WithBaseCurrency(string baseCurrency)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency))
                throw new ArgumentException("Base currency cannot be null or empty", nameof(baseCurrency));

            if (_restrictedCurrencies.Contains(baseCurrency))
                throw new DomainException($"Currency {baseCurrency} is restricted");

            _baseCurrency = baseCurrency;
            return this;
        }

        public ExchangeRateBuilder WithTimestamp(DateTime timestamp)
        {
            _timestamp = timestamp;
            return this;
        }

        public ExchangeRateBuilder UseCurrentTimestamp()
        {
            _timestamp = DateTime.UtcNow;
            return this;
        }

        public ExchangeRateBuilder WithSource(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Source cannot be null or empty", nameof(source));

            _source = source;
            return this;
        }

        public ExchangeRateBuilder AddRate(string currency, decimal rate)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

            if (_restrictedCurrencies.Contains(currency))
                throw new DomainException($"Currency {currency} is restricted");

            if (rate <= 0)
                throw new ArgumentException("Rate must be greater than zero", nameof(rate));

            _rates[currency] = rate;
            return this;
        }

        public ExchangeRateBuilder AddRates(IDictionary<string, decimal> rates)
        {
            if (rates == null)
                throw new ArgumentNullException(nameof(rates));

            foreach (var rate in rates)
            {
                AddRate(rate.Key, rate.Value);
            }

            return this;
        }

        public ExchangeRateRecord Build()
        {
            if (string.IsNullOrWhiteSpace(_baseCurrency))
                throw new InvalidOperationException("Base currency must be set");

            if (_timestamp == default)
                _timestamp = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(_source))
                throw new InvalidOperationException("Source must be set");

            var exchangeRate = new ExchangeRateRecord(_baseCurrency, _timestamp, _source);

            foreach (var rate in _rates)
            {
                exchangeRate.AddRate(rate.Key, rate.Value);
            }

            return exchangeRate;
        }

        public static ExchangeRateBuilder Create()
        {
            return new ExchangeRateBuilder();
        }
    }
}
