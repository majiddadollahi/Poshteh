using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IServiceProvider serviceProvider, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        var validators = _serviceProvider.GetServices<IValidator<TRequest>>();

        foreach (var validator in validators)
        {
            var validationResult = validator.Validate(request);
            if (validationResult.IsFailure)
            {
                _logger.LogWarning("Validation failed for {RequestType}. Reason: {Reason}", typeof(TRequest).Name, validationResult.Error.Message);
                return Result.Failure<TResponse>(validationResult.Error);
            }
        }

        return await next();
    }
}
