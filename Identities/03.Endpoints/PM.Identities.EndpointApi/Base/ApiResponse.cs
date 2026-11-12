namespace PM.Identities.EndpointApi.Base;

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public List<ApiError>? Errors { get; set; }

    public static ApiResponse Ok(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public static ApiResponse Fail(
        string message,
        List<ApiError>? errors = null,
        string? code = null)
    {
        return new ApiResponse
        {
            Success = false,
            Code = code,
            Message = message,
            Errors = errors
        };
    }
}
