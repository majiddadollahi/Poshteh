using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Poshteh.Core.Contract.Localization;
using Poshteh.Core.Domain;
using Poshteh.Endpoint.Api.Responses;

namespace Poshteh.Endpoint.Api.Controllers;

/// <summary>
/// Controller base that wraps every response in <see cref="ApiResponse"/>/<see cref="ApiResponse{T}"/> with
/// localized messages. Unlike <see cref="BaseController"/> it needs no dispatchers; the
/// <see cref="IJsonMessageLocalizer"/> (registered by <c>AddPoshtehJsonLocalization</c>) is resolved from
/// <c>HttpContext.RequestServices</c>, so derived controllers keep their own constructors.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IJsonMessageLocalizer Localizer =>
        HttpContext.RequestServices.GetRequiredService<IJsonMessageLocalizer>();

    protected IActionResult HandleResult<T>(Result<T> result, string? successMessageCode = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? OkResponse(result.Value, successMessageCode) : FailFromError(result.Error);
    }

    protected IActionResult HandleResult(Result result, string? successMessageCode = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? OkResponse(successMessageCode) : FailFromError(result.Error);
    }

    protected IActionResult OkResponse<T>(T data, string? messageCode = null) =>
        Ok(ApiResponse<T>.Ok(data, LocalizeOptional(messageCode), messageCode));

    protected IActionResult OkResponse(string? messageCode = null) =>
        Ok(ApiResponse.Ok(LocalizeOptional(messageCode), messageCode));

    protected IActionResult FailResponse(string messageCode, List<ApiError>? errors = null, int statusCode = 400)
    {
        var message = Localizer.Get(messageCode);
        errors ??= [new ApiError { Code = messageCode, Message = message }];
        return StatusCode(statusCode, ApiResponse<object>.Fail(message, messageCode, errors));
    }

    protected IActionResult FailFromError(Error error, int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(error);

        var message = Localizer.Get(error);
        var body = ApiResponse<object>.Fail(
            message,
            error.Code,
            [new ApiError { Code = error.Code, Message = message }]);

        return StatusCode(statusCode ?? ApiErrorStatusCodes.Resolve(error.Code), body);
    }

    private string? LocalizeOptional(string? messageCode) =>
        string.IsNullOrWhiteSpace(messageCode) ? null : Localizer.Get(messageCode);
}
