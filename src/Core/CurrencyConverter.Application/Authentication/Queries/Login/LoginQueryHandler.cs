using CurrencyConverter.Application.Common.Abstractions;
using CurrencyConverter.Dtos.Authentication;
using ErrorOr;
using MediatR;

namespace CurrencyConverter.Application.Authentication.Queries.Login
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, ErrorOr<AuthenticationResult>>
    {
        private readonly IUserRepository _userRepository;

        public LoginQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ErrorOr<AuthenticationResult>> Handle(LoginQuery query, CancellationToken cancellationToken)
        {
            return await _userRepository.LoginAsync(query.Email, query.Password);
        }
    }
}
