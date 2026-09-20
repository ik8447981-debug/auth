namespace LicensePlatform.Shared.DTOs.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public string? RequestId { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null, string? requestId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            RequestId = requestId
        };
    }

    public static ApiResponse<T> Fail(string error, string? message = null, string? requestId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error,
            Message = message,
            RequestId = requestId
        };
    }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public string? RequestId { get; set; }

    public static ApiResponse Ok(string? message = null, string? requestId = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            RequestId = requestId
        };
    }

    public static ApiResponse Fail(string error, string? message = null, string? requestId = null)
    {
        return new ApiResponse
        {
            Success = false,
            Error = error,
            Message = message,
            RequestId = requestId
        };
    }
}
