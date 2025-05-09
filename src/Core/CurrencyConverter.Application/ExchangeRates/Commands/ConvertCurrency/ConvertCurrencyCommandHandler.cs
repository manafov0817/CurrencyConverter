using CurrencyConverter.Application.Common.Abstractions;
using CurrencyConverter.Domain.Common;
using CurrencyConverter.Dtos.ExchangeRates;
using ErrorOr;
using MediatR;

namespace CurrencyConverter.Application.ExchangeRates.Commands.ConvertCurrency
{
    public class ConvertCurrencyCommandHandler : IRequestHandler<ConvertCurrencyCommand, ErrorOr<ConversionResponse>>
    {
        private readonly ICurrencyProviderFactory _currencyProviderFactory;

        public ConvertCurrencyCommandHandler(ICurrencyProviderFactory currencyProviderFactory)
        {
            _currencyProviderFactory = currencyProviderFactory;
        }

        public async Task<ErrorOr<ConversionResponse>> Handle(ConvertCurrencyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var provider = _currencyProviderFactory.GetProvider(request.ProviderName);

                // Get latest exchange rates to calculate the rate between source and target
                var baseRates = await provider.GetLatestRatesAsync(request.FromCurrency);
                var convertedAmount = await provider.ConvertCurrencyAsync(
                    request.Amount, request.FromCurrency, request.ToCurrency);

                decimal exchangeRate = 1;
                if (request.FromCurrency != request.ToCurrency)
                {
                    exchangeRate = baseRates.GetConversionRate(request.ToCurrency);
                }

                var response = new ConversionResponse
                {
                    OriginalAmount = request.Amount,
                    OriginalCurrency = request.FromCurrency,
                    ConvertedAmount = convertedAmount,
                    TargetCurrency = request.ToCurrency,
                    ExchangeRate = exchangeRate,
                    Timestamp = baseRates.Timestamp
                };

                return response;
            }
            catch (DomainException ex)
            {
                return Error.Validation(ex.Message);
            }
            catch (Exception ex)
            {
                return Error.Failure("ExchangeRates.ConvertCurrencyFailed", ex.Message);
            }
        }
    }
}
