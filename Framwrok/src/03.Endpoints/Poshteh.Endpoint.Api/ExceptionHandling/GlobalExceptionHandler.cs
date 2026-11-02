using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Poshteh.Core.ApplicationService.Localization;
using Poshteh.Core.Contract.Localization;

namespace Poshteh.Endpoint.Api.ExceptionHandling;

/// <summary>
/// Last-resort safety net for exceptions that escape the CQRS pipeline (routing, model binding, middleware,
/// or a bug in a handler that bypassed <c>Result</c>). Every expected failure should already have been mapped
/// to a <c>Result</c>/<c>Error</c> by <c>ExceptionHandlingBehavior</c> long before reaching here — anything that
/// does reach here is, by definition, unexpected, so the response stays generic and leaks no internal detail.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IMessageLocalizer localizer,
    MessageLocalizationOptions localizationOptions) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var code = localizationOptions.UnexpectedErrorCode;
        await Microsoft.AspNetCore.Http.Results.Problem(
                detail: localizer.Localize(code),
                statusCode: StatusCodes.Status500InternalServerError,
                title: code,
                extensions: new Dictionary<string, object?> { ["code"] = code })
            .ExecuteAsync(httpContext);

        return true;
    }
}
