using Microsoft.Extensions.Logging;
using Poshteh.Core.ApplicationService.Localization;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Behaviors;

public sealed class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;
    private readonly string _unexpectedErrorCode;

    public ExceptionHandlingBehavior(
        ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger,
        MessageLocalizationOptions? localizationOptions = null)
    {
        _logger = logger;
        _unexpectedErrorCode = localizationOptions?.UnexpectedErrorCode ?? "ApplicationException.Unhandled";
    }

    public async Task<Result<TResponse>> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        try
        {
            return await next();
        }
        catch (DomainException domainException)
        {
            _logger.LogWarning(domainException, "Domain exception raised for {RequestType}", typeof(TRequest).Name);
            // The exception's own message is never returned to the client: Error defaults Message to
            // Code when none is supplied, and the localizer treats "message == code" as unsafe to show
            // verbatim, so this always resolves through the safe unexpected-error fallback instead.
            return Result.Failure<TResponse>("DomainException");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected exception for {RequestType}", typeof(TRequest).Name);
            return Result.Failure<TResponse>(_unexpectedErrorCode);
        }
    }
}
