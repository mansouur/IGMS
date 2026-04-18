namespace IGMS.Domain.Common;

/// <summary>
/// Unified API response envelope for all endpoints.
/// Ensures consistent shape for React and Flutter consumers.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];
    public int StatusCode { get; set; }
    public PaginationMeta? Pagination { get; set; }

    public static ApiResponse<T> Ok(T? data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        StatusCode = 200
    };

    public static ApiResponse<T> Created(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        StatusCode = 201
    };

    public static ApiResponse<T> Fail(string error, int statusCode = 400) => new()
    {
        Success = false,
        Errors = [error],
        StatusCode = statusCode
    };

    public static ApiResponse<T> Fail(List<string> errors, int statusCode = 400) => new()
    {
        Success = false,
        Errors = errors,
        StatusCode = statusCode
    };

    public static ApiResponse<T> NotFound(string message) =>
        Fail(message, 404);

    public static ApiResponse<T> Unauthorized(string message = "Unauthorized") =>
        Fail(message, 401);
}

public class PaginationMeta
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
}
