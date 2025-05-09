using CurrencyConverter.Application.Common.Abstractions;
using CurrencyConverter.Domain.Common;
using CurrencyConverter.Dtos.ExchangeRates;
using ErrorOr;
using MediatR;

namespace CurrencyConverter.Application.ExchangeRates.Queries.GetHistoricalRates
{
    public class GetHistoricalRatesQueryHandler : IRequestHandler<GetHistoricalRatesQuery, ErrorOr<HistoricalRatesResponse>>
    {
        private readonly ICurrencyProviderFactory _currencyProviderFactory;

        public GetHistoricalRatesQueryHandler(ICurrencyProviderFactory currencyProviderFactory)
        {
            _currencyProviderFactory = currencyProviderFactory;
        }

        public async Task<ErrorOr<HistoricalRatesResponse>> Handle(GetHistoricalRatesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.StartDate > request.EndDate)
                    return Error.Validation("Start date must be before or equal to end date");

                if (request.Page < 1)
                    return Error.Validation("Page must be greater than or equal to 1");

                if (request.PageSize < 1 || request.PageSize > 100)
                    return Error.Validation("Page size must be between 1 and 100");

                var provider = _currencyProviderFactory.GetProvider(request.ProviderName);
                var (rates, totalCount) = await provider.GetHistoricalRatesAsync(
                    request.BaseCurrency,
                    request.StartDate,
                    request.EndDate,
                    request.Page,
                    request.PageSize);

                int totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                var response = new HistoricalRatesResponse
                {
                    BaseCurrency = request.BaseCurrency,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    Rates = rates.Select(r => new DailyRate
                    {
                        Date = r.Timestamp,
                        Rates = r.Rates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    }).ToList()
                };

                return response;
            }
            catch (DomainException ex)
            {
                return Error.Validation(ex.Message);
            }
            catch (Exception ex)
            {
                return Error.Failure("ExchangeRates.GetHistoricalRatesFailed", ex.Message);
            }
        }
    }
}
