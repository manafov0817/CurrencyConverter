using CurrencyConverter.Application.Common.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers
{
   
    // Ignore by swagger
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ErrorsController : Controller
    {
        [Route("/error")]
        public IActionResult Index()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>().Error;

            var (statusCode, message) = exception switch
            {
                IServiceException serviceException => ((int)serviceException.StatusCode, serviceException.ErrorMessage),
                _ => (StatusCodes.Status500InternalServerError, exception.Message)
            };
            return Problem(statusCode: statusCode, title: message);
        }
    }
}
