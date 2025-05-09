using CurrencyConverter.Domain.ExchangeRates;

namespace CurrencyConverter.Application.Common.Abstractions
{
    public interface ICurrencyProvider
    {
        Task<ExchangeRateRecord> GetLatestRatesAsync(string baseCurrency);

        Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);

        Task<(IEnumerable<ExchangeRateRecord> Rates, int TotalCount)> GetHistoricalRatesAsync(
            string baseCurrency,
            DateTime startDate,
            DateTime endDate,
            int page,
            int pageSize);
    }
}
