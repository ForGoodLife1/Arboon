using System.Text.Json.Serialization;

namespace Arboon.Application.Common;

/// <summary>
/// Unified API response wrapper for consistent JSON output.
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiError? Error { get; set; }

    public static ApiResponse<T> SuccessResponse(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ApiResponse<T> ErrorResponse(string code, string message) => new()
    {
        Success = false,
        Error = new ApiError { Code = code, Message = message }
    };
}

public class ApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiError? Error { get; set; }

    public static ApiResponse SuccessResponse(string? message = null, object? data = null) => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponse ErrorResponse(string code, string message) => new()
    {
        Success = false,
        Error = new ApiError { Code = code, Message = message }
    };
}

public class ApiError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
