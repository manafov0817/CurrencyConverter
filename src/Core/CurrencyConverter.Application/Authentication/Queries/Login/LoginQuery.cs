using CurrencyConverter.Dtos.Authentication;
using ErrorOr;
using MediatR;

namespace CurrencyConverter.Application.Authentication.Queries.Login
{
    public record LoginQuery(string Email, string Password) : IRequest<ErrorOr<AuthenticationResult>>;
}
