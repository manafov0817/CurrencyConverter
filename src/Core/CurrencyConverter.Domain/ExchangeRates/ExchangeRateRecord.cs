using CurrencyConverter.Domain.Common;

namespace CurrencyConverter.Domain.ExchangeRates
{
    public class ExchangeRateRecord : AggregateRoot
    {
        public string BaseCurrency { get; private set; }
        private readonly HashSet<string> _restrictedCurrencies = new HashSet<string> { "TRY", "PLN", "THB", "MXN" };
        private readonly Dictionary<string, decimal> _rates = new Dictionary<string, decimal>();
        public IReadOnlyDictionary<string, decimal> Rates => _rates;
        public DateTime Timestamp { get; private set; }
        public string Source { get; private set; }

        private ExchangeRateRecord() { } // For ORM

        public ExchangeRateRecord(string baseCurrency, DateTime timestamp, string source)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency))
                throw new ArgumentException("Base currency cannot be null or empty", nameof(baseCurrency));

            if (_restrictedCurrencies.Contains(baseCurrency))
                throw new DomainException($"Currency {baseCurrency} is restricted");

            BaseCurrency = baseCurrency;
            Timestamp = timestamp;
            Source = source;
            Id = Guid.NewGuid();
        }

        public void AddRate(string currency, decimal rate)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

            if (_restrictedCurrencies.Contains(currency))
                throw new DomainException($"Currency {currency} is restricted");

            if (rate <= 0)
                throw new ArgumentException("Rate must be greater than zero", nameof(rate));

            _rates[currency] = rate;
        }

        public bool HasCurrency(string currency)
        {
            return BaseCurrency.Equals(currency, StringComparison.OrdinalIgnoreCase) ||
                   _rates.ContainsKey(currency);
        }

        public decimal GetConversionRate(string targetCurrency)
        {
            if (BaseCurrency.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase))
                return 1m;

            if (!_rates.TryGetValue(targetCurrency, out decimal rate))
                throw new DomainException($"No rate found for {targetCurrency}");

            return rate;
        }
    }
}
