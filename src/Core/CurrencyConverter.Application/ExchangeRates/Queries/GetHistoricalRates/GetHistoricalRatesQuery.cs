using CurrencyConverter.Dtos.ExchangeRates;
using ErrorOr;
using MediatR;

namespace CurrencyConverter.Application.ExchangeRates.Queries.GetHistoricalRates
{
    public record GetHistoricalRatesQuery(
        string BaseCurrency,
        DateTime StartDate,
        DateTime EndDate,
        int Page = 1,
        int PageSize = 10,
        string ProviderName = null) : IRequest<ErrorOr<HistoricalRatesResponse>>;
}
