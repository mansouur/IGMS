namespace IGMS.Domain.Common;

/// <summary>
/// Result pattern for service layer – avoids throwing exceptions for business logic failures.
/// Use this in Application/Infrastructure; convert to ApiResponse at the controller level.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }

    private Result() { }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };

    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };

    // Implicit conversions for cleaner controller code
    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Non-generic Result for operations that return no value (void-equivalent).
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }

    private Result() { }

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(string error) => new() { IsSuccess = false, Error = error };
}
