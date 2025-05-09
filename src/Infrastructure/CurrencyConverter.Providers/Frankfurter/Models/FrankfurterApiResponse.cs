using System.Text.Json.Serialization;

namespace CurrencyConverter.Providers.ExchangeRates.Models
{
    public class FrankfurterApiResponse
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("base")]
        public string Base { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; }
    }

    public class FrankfurterHistoricalResponse
    {
        [JsonPropertyName("base")]
        public string Base { get; set; }

        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
    }
}
