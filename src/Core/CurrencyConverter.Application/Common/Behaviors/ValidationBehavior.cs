using ErrorOr;
using FluentValidation;
using MediatR; 

namespace CurrencyConverter.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IValidator<TRequest>? _validator;

        public ValidationBehavior(IValidator<TRequest>? validator = null)
        {
            _validator = validator;
        }

        public async Task<TResponse> Handle(TRequest request,
                                      RequestHandlerDelegate<TResponse> next,
                                      CancellationToken cancellationToken)
        {
            if (_validator is null) return await next();

            var validationResult = await _validator.ValidateAsync(request);

            if (validationResult.IsValid) return await next();

            var errors = validationResult.Errors.Select(validationFailure => Error.Validation(validationFailure.PropertyName,
                                                                                              validationFailure.ErrorMessage))
                                                .ToList();

            return (dynamic)errors;
        }
    }
}
