namespace TelemetryHub.Api;

public sealed class Result<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public object? ValidationErrors { get; init; }

    public static Result<T> Ok(T data) => new() { Success = true, Data = data };
    public static Result<T> Fail(string error) => new() { Success = false, Error = error };
    public static Result<T> Fail(string error, object validationErrors) =>
        new() { Success = false, Error = error, ValidationErrors = validationErrors };
}
