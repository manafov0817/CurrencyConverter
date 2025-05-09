namespace CurrencyConverter.Dtos.ExchangeRates
{
    public class LatestRatesResponse
    {
        public string BaseCurrency { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
        public string Source { get; set; }
    }
}
