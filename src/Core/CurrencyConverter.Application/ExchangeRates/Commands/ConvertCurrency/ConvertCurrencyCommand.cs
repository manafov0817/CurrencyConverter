using CurrencyConverter.Dtos.ExchangeRates;
using ErrorOr;
using MediatR;

namespace CurrencyConverter.Application.ExchangeRates.Commands.ConvertCurrency
{
    public record ConvertCurrencyCommand(
        decimal Amount,
        string FromCurrency,
        string ToCurrency,
        string ProviderName = null) : IRequest<ErrorOr<ConversionResponse>>;
}
