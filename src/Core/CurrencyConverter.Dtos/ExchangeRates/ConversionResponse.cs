namespace CurrencyConverter.Dtos.ExchangeRates
{
    public class ConversionResponse
    {
        public decimal OriginalAmount { get; set; }
        public string OriginalCurrency { get; set; }
        public decimal ConvertedAmount { get; set; }
        public string TargetCurrency { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
