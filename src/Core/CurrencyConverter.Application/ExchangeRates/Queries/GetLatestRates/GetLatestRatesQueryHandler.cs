using CurrencyConverter.Application.Common.Abstractions;
using CurrencyConverter.Domain.Common;
using CurrencyConverter.Dtos.ExchangeRates;
using ErrorOr;
using MediatR;

namespace CurrencyConverter.Application.ExchangeRates.Queries.GetLatestRates
{
    public class GetLatestRatesQueryHandler : IRequestHandler<GetLatestRatesQuery, ErrorOr<LatestRatesResponse>>
    {
        private readonly ICurrencyProviderFactory _currencyProviderFactory;

        public GetLatestRatesQueryHandler(ICurrencyProviderFactory currencyProviderFactory)
        {
            _currencyProviderFactory = currencyProviderFactory;
        }

        public async Task<ErrorOr<LatestRatesResponse>> Handle(GetLatestRatesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var provider = _currencyProviderFactory.GetProvider(request.ProviderName);
                var exchangeRate = await provider.GetLatestRatesAsync(request.BaseCurrency);

                var response = new LatestRatesResponse
                {
                    BaseCurrency = exchangeRate.BaseCurrency,
                    Timestamp = exchangeRate.Timestamp,
                    Rates = exchangeRate.Rates.ToDictionary(r => r.Key, r => r.Value),
                    Source = exchangeRate.Source
                };

                return response;
            }
            catch (DomainException ex)
            {
                return Error.Validation(ex.Message);
            }
            catch (Exception ex)
            {
                return Error.Failure("ExchangeRates.GetLatestRatesFailed", ex.Message);
            }
        }
    }
}
