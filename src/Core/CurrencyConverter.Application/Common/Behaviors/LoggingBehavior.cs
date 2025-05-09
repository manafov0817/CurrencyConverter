using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CurrencyConverter.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggingBehavior(
            ILogger<LoggingBehavior<TRequest, TResponse>> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var httpMethod = _httpContextAccessor.HttpContext?.Request?.Method;
            var endpoint = _httpContextAccessor.HttpContext?.Request?.Path;

            _logger.LogInformation(
                "Processing request {RequestName}. ClientIP: {ClientIP}, Method: {Method}, Endpoint: {Endpoint}",
                requestName, clientIp, httpMethod, endpoint);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();

                stopwatch.Stop();

                _logger.LogInformation(
                    "Request {RequestName} processed successfully. ResponseTime: {ResponseTime}ms",
                    requestName, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "Request {RequestName} processing failed. Error: {Error}, ResponseTime: {ResponseTime}ms",
                    requestName, ex.Message, stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}
