namespace CurrencyConverter.Dtos.ExchangeRates
{
    public class HistoricalRatesResponse
    {
        public string BaseCurrency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<DailyRate> Rates { get; set; }
    }

    public class DailyRate
    {
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
