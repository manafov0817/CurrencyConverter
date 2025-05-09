using CurrencyConverter.Dtos.Authentication;
using ErrorOr;
using MediatR;

namespace CurrencyConverter.Application.Authentication.Commands.Register
{
    public record RegisterCommand(string Username, 
                                  string Email,
                                  string Password) : IRequest<ErrorOr<AuthenticationResult>>;
}

