using CurrencyConverter.Domain.Users;
using CurrencyConverter.Dtos.Authentication;
using ErrorOr;

namespace CurrencyConverter.Application.Common.Abstractions
{
    public interface IUserRepository
    {
        Task<ErrorOr<AuthenticationResult>> LoginAsync(string email, string password);
        Task<ErrorOr<AuthenticationResult>> RegisterAsync(User user);
    }
}
