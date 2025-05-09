using CurrencyConverter.Dtos.ExchangeRates;
using ErrorOr;
using MediatR;

namespace CurrencyConverter.Application.ExchangeRates.Queries.GetLatestRates
{
    public record GetLatestRatesQuery(
        string BaseCurrency = "EUR",
        string ProviderName = null) : IRequest<ErrorOr<LatestRatesResponse>>;
}
