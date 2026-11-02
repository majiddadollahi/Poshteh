using Microsoft.Extensions.Logging;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        var requestType = typeof(TRequest).Name;
        _logger.LogInformation("Starting {RequestType}", requestType);

        try
        {
            var result = await next();
            if (result.IsSuccess)
            {
                _logger.LogInformation("Completed {RequestType} successfully", requestType);
            }
            else
            {
                _logger.LogWarning("Completed {RequestType} with failure: {Reason}", requestType, result.Error.Message);
            }

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception in {RequestType}", requestType);
            throw;
        }
    }
}
