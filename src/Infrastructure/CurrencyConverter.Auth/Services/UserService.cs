using CurrencyConverter.Application.Common.Abstractions;
using CurrencyConverter.Domain.Users;
using CurrencyConverter.Dtos.Authentication;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Auth.Services
{
    internal class UserService : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenGeneratorService _tokenGenerator;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IConfiguration configuration,
            ITokenGeneratorService tokenGenerator,
            ILogger<UserService> logger)
        {
            _configuration = configuration;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        public async Task<ErrorOr<AuthenticationResult>> LoginAsync(string email, string password)
        {
            var adminUser = _configuration.GetSection("Auth:AdminUser");
            var demoUser = _configuration.GetSection("Auth:DemoUser");

            if (string.IsNullOrEmpty(adminUser["Email"]) || string.IsNullOrEmpty(adminUser["Password"]) ||
                string.IsNullOrEmpty(demoUser["Email"]) || string.IsNullOrEmpty(demoUser["Password"]))
            {
                _logger.LogError("User credentials not configured properly");
                return Error.Failure("Auth.MissingConfiguration", "Authentication service misconfigured");
            }

            if (email == adminUser["Email"] && password == adminUser["Password"])
            {
                var accessToken = _tokenGenerator.GenerateToken(email, adminUser["Role"]);
                var refreshToken = Guid.NewGuid().ToString(); // In a real app, this would be persisted
                return new AuthenticationResult(accessToken, refreshToken);
            }
            else if (email == demoUser["Email"] && password == demoUser["Password"])
            {
                var accessToken = _tokenGenerator.GenerateToken(email, demoUser["Role"]);
                var refreshToken = Guid.NewGuid().ToString(); // In a real app, this would be persisted
                return new AuthenticationResult(accessToken, refreshToken);
            }

            _logger.LogWarning("Failed login attempt for user: {Email}", email);
            return Error.NotFound("Auth.InvalidCredentials", "Invalid email or password");
        }

        public async Task<ErrorOr<AuthenticationResult>> RegisterAsync(User user)
        {
            // For this implementation, we'll just return a success message with a token
            // In a real-world scenario, we would store the user in a database
            _logger.LogInformation("Registering new user: {Email}", user.Email);

            var accessToken = _tokenGenerator.GenerateToken(user.Email, "User");
            var refreshToken = Guid.NewGuid().ToString(); // In a real app, this would be persisted
            return new AuthenticationResult(accessToken, refreshToken);
        }
    }
}
