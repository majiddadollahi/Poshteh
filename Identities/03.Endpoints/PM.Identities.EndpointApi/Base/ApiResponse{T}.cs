namespace PM.Identities.EndpointApi.Base;

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static new ApiResponse<T> Fail(
        string message,
        List<ApiError>? errors = null,
        string? code = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Code = code,
            Message = message,
            Errors = errors
        };
    }
}
