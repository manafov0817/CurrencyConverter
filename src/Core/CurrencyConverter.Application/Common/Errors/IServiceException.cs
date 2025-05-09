using System.Net;

namespace CurrencyConverter.Application.Common.Errors
{
    public interface IServiceException
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorMessage { get; }
    }
}
