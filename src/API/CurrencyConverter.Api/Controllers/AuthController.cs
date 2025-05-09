using CurrencyConverter.Application.Authentication.Commands.Register;
using CurrencyConverter.Application.Authentication.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiVersion("1.0")]
    public class AuthController : ApiController
    {
        private readonly ISender _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ISender mediator,
                              ILogger<AuthController> logger )
        {
            _logger = logger;
            _mediator = mediator; ;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            _logger.LogInformation($"/register called, Registering user with email {command.Email}");

            var authenticationResult = await _mediator.Send(command);

            return authenticationResult.Match(result => Ok(result.AccessToken),
                                              Problem);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginQuery query)
        {
            _logger.LogInformation($"/login called, Logging in user with email {query.Email}");

            var authenticationResult = await _mediator.Send(query);

            return authenticationResult.Match(Ok, Problem);
        }

    }
}
