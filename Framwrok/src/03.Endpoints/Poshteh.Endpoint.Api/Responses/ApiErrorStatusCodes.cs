using Microsoft.AspNetCore.Http;

namespace Poshteh.Endpoint.Api.Responses;

/// <summary>
/// Error-code → HTTP status mapping used by <see cref="Controllers.BaseApiController"/> (ordinal substring
/// match, first rule wins). Independent of <c>ToApiResult</c>'s ProblemDetails mapping.
/// </summary>
public static class ApiErrorStatusCodes
{
    public static int Resolve(string? errorCode)
    {
        var code = errorCode ?? string.Empty;

        if (code.Contains("NotFound", StringComparison.Ordinal))
        {
            return StatusCodes.Status404NotFound;
        }

        if (code.Contains("Unauthorized", StringComparison.Ordinal) ||
            code.Contains("InvalidCredentials", StringComparison.Ordinal))
        {
            return StatusCodes.Status401Unauthorized;
        }

        if (code.Contains("Forbidden", StringComparison.Ordinal) ||
            code.Contains("AccessDenied", StringComparison.Ordinal))
        {
            return StatusCodes.Status403Forbidden;
        }

        if (code.Contains("Conflict", StringComparison.Ordinal) ||
            code.Contains("AlreadyExists", StringComparison.Ordinal))
        {
            return StatusCodes.Status409Conflict;
        }

        return StatusCodes.Status400BadRequest;
    }
}
