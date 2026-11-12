using BuildingBlocks.Web.Controllers;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.ApplicationService.Queries;
using Poshteh.Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Poshteh.Core.Contract.Localization;

namespace PM.Identities.EndpointApi.Base;

[ApiController]
public abstract class BaseApiController : CqrsApiController
{
    protected BaseApiController(
        ICommandDispatcher? commandDispatcher = null,
        IQueryDispatcher? queryDispatcher = null)
        : base(commandDispatcher, queryDispatcher)
    {
    }

    protected override IActionResult HandleResult<T>(Result<T> result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Ok(result.Value, LocalizeSuccessMessage(successMessage)));
        }

        return HandleError(result.Error);
    }

    protected override IActionResult HandleResult(Result result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse.Ok(LocalizeSuccessMessage(successMessage)));
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Resolves a success-message key (e.g. "Identity.User.Created") to its localized text.
    /// Never falls back to the generic unexpected-error message: when no resource exists yet for
    /// the key, the raw key is kept as-is (today's behavior) rather than claiming something failed.
    /// </summary>
    private string? LocalizeSuccessMessage(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return key;
        }

        var localizer = HttpContext?.RequestServices?.GetService<IMessageLocalizer>();
        return localizer?.TryLocalize(key) ?? key;
    }

    protected IActionResult OkResponse<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult OkResponse(string? message = null)
        => Ok(ApiResponse.Ok(message));

    protected IActionResult FailResponse(
        string message,
        List<ApiError>? errors = null,
        int statusCode = StatusCodes.Status400BadRequest)
        => StatusCode(
            statusCode,
            ApiResponse<object>.Fail(message, errors, errors?.FirstOrDefault()?.Code));

    private IActionResult HandleError(Error error)
    {
        var statusCode = GetStatusCode(error);
        var message = HttpContext.RequestServices
            .GetRequiredService<IMessageLocalizer>()
            .Localize(error);
        var apiError = new ApiError
        {
            Code = error.Code,
            Message = message
        };

        return StatusCode(
            statusCode,
            ApiResponse<object>.Fail(message, new List<ApiError> { apiError }, error.Code));
    }

    private static int GetStatusCode(Error error)
    {
        return error.Code switch
        {
            var code when code.Contains("NotFound") => StatusCodes.Status404NotFound,
            var code when code.Contains("Unauthorized") || code.Contains("InvalidCredentials") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            var code when code.Contains("Conflict") || code.Contains("AlreadyExists") => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
