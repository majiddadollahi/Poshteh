using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Poshteh.Core.ApplicationService.Validations;
using Poshteh.Core.Contract.Localization;
using Poshteh.Endpoint.Api.Responses;

namespace Poshteh.Endpoint.Api.Filters;

/// <summary>
/// Turns a <see cref="ValidationException"/> into a 400 <see cref="ApiResponse{T}"/> whose errors carry each
/// failure's message code and localized text. Other exceptions are left alone.
/// Register with <c>services.AddControllers(o =&gt; o.Filters.Add&lt;LocalizationValidationExceptionFilter&gt;())</c>.
/// </summary>
public sealed class LocalizationValidationExceptionFilter(IJsonMessageLocalizer localizer) : IExceptionFilter
{
    public const string DefaultCode = "Validation.Failed";

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ValidationException validationException)
        {
            return;
        }

        var errors = validationException.Errors
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? DefaultCode : error.ErrorMessage)
            .Select(code => new ApiError { Code = code, Message = localizer.Get(code) })
            .ToList();

        var topCode = errors.Count > 0 ? errors[0].Code : DefaultCode;
        var topMessage = errors.Count > 0 ? errors[0].Message : localizer.Get(DefaultCode);

        context.Result = new ObjectResult(ApiResponse<object>.Fail(topMessage, topCode, errors))
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
        context.ExceptionHandled = true;
    }
}
