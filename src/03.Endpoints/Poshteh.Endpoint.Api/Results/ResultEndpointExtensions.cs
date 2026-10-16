using Microsoft.AspNetCore.Http;
using Poshteh.Core.Contract.Localization;
using Poshteh.Core.Domain;

namespace Poshteh.Endpoint.Api.Results;

/// <summary>
/// Maps <see cref="Result"/>/<see cref="Result{TValue}"/> — the return type every Command/Query handler uses —
/// to an HTTP response, so endpoints never hand-roll status-code logic per use case. Success always maps to a
/// 2xx; a specific "created"/"no content" shape is still the endpoint's call (that needs a Location header or
/// simply has no natural success payload) — this only owns the generic default and every failure path.
/// </summary>
public static class ResultEndpointExtensions
{
    public static IResult ToApiResult(this Result result, IMessageLocalizer localizer) =>
        result.IsSuccess ? Microsoft.AspNetCore.Http.Results.NoContent() : ToProblem(result.Error, localizer);

    public static IResult ToApiResult<TValue>(this Result<TValue> result, IMessageLocalizer localizer) =>
        result.IsSuccess ? Microsoft.AspNetCore.Http.Results.Ok(result.Value) : ToProblem(result.Error, localizer);

    [Obsolete("Pass the configured IMessageLocalizer so failures use application resources.")]
    public static IResult ToApiResult(this Result result) =>
        result.IsSuccess ? Microsoft.AspNetCore.Http.Results.NoContent() : ToSafeProblem(result.Error);

    [Obsolete("Pass the configured IMessageLocalizer so failures use application resources.")]
    public static IResult ToApiResult<TValue>(this Result<TValue> result) =>
        result.IsSuccess ? Microsoft.AspNetCore.Http.Results.Ok(result.Value) : ToSafeProblem(result.Error);

    private static IResult ToProblem(Error error, IMessageLocalizer localizer)
    {
        var statusCode = ResolveStatusCode(error.Code);

        return Microsoft.AspNetCore.Http.Results.Problem(
            detail: localizer.Localize(error),
            statusCode: statusCode,
            title: error.Code,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }

    private static IResult ToSafeProblem(Error error) =>
        Microsoft.AspNetCore.Http.Results.Problem(
            detail: "An unexpected error occurred.",
            statusCode: ResolveStatusCode(error.Code),
            title: error.Code,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });

    private static int ResolveStatusCode(string code)
    {
        if (code.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCodes.Status404NotFound;
        }

        if (code.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCodes.Status409Conflict;
        }

        if (code.Contains("Validation", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("DomainException", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCodes.Status422UnprocessableEntity;
        }

        return StatusCodes.Status400BadRequest;
    }
}
