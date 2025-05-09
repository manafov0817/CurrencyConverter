using CurrencyConverter.Application.ExchangeRates.Commands.ConvertCurrency;
using CurrencyConverter.Application.ExchangeRates.Queries.GetHistoricalRates;
using CurrencyConverter.Application.ExchangeRates.Queries.GetLatestRates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ExchangesController : ApiController
    {
        private readonly ISender _mediator;

        public ExchangesController(ISender mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet("latest")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetLatestRates([FromQuery] GetLatestRatesQuery query)
        {
            var result = await _mediator.Send(query);

            return result.Match(Ok, Problem);
        }

        [HttpPost("convert")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> ConvertCurrency([FromBody] ConvertCurrencyCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);

            return result.Match(Ok, Problem);
        }

        [HttpGet("historical")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetHistoricalRates([FromQuery] GetHistoricalRatesQuery query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(query);

            return result.Match(Ok, Problem);
        }
    }
}
