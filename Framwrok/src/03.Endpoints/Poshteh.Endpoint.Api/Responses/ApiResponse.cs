namespace Poshteh.Endpoint.Api.Responses;

/// <summary>
/// Response envelope returned by <see cref="Controllers.BaseApiController"/>. The JSON shape
/// (<c>success</c>, <c>message</c>, <c>code</c>, <c>errors</c>, <c>data</c>) is a frontend contract; do not rename.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }

    /// <summary>Localized text.</summary>
    public string? Message { get; set; }

    /// <summary>Stable message code.</summary>
    public string? Code { get; set; }

    public List<ApiError>? Errors { get; set; }

    public static ApiResponse Ok(string? message = null, string? code = null) =>
        new() { Success = true, Message = message, Code = code };

    public static ApiResponse Fail(string message, string? code = null, List<ApiError>? errors = null) =>
        new() { Success = false, Message = message, Code = code, Errors = errors };
}

// Static factories on the generic type are the intended, frontend-facing API (ApiResponse<T>.Ok(...)).
#pragma warning disable CA1000
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null, string? code = null) =>
        new() { Success = true, Data = data, Message = message, Code = code };

    public static new ApiResponse<T> Fail(string message, string? code = null, List<ApiError>? errors = null) =>
        new() { Success = false, Message = message, Code = code, Errors = errors };
}

#pragma warning restore CA1000

public class ApiError
{
    public string Code { get; set; } = "";

    public string Message { get; set; } = "";
}
