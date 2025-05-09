using CurrencyConverter.Application.Common.Abstractions;
using CurrencyConverter.Domain.Users.Builders;
using CurrencyConverter.Dtos.Authentication;
using ErrorOr;
using MediatR;

namespace CurrencyConverter.Application.Authentication.Commands.Register
{
    public class RegisterCommandHandler(IUserRepository userRepository) : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<ErrorOr<AuthenticationResult>> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            var user = UserBuilder.Create()
                                  .WithUsername(command.Username)
                                  .WithEmail(command.Email)
                                  .WithPassword(command.Password)
                                  .Build();

            return await _userRepository.RegisterAsync(user);
        }
    }
}
