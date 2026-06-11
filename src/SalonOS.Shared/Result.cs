namespace SalonOS.Shared;

/// <summary>
/// Monadic result type for error handling without exceptions.
/// Use Result.Ok(value) for success, Result.Error(message) for failure.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }

    private Result(T? value, string? errorMessage, bool isSuccess)
    {
        Value = value;
        ErrorMessage = errorMessage;
        IsSuccess = isSuccess;
    }

    public static Result<T> Ok(T value) => new(value, null, true);
    public static Result<T> Fail(string error) => new(default, error, false);

    public Result<TOut> Map<TOut>(Func<T, TOut> map)
    {
        return IsSuccess
            ? Result<TOut>.Ok(map(Value!))
            : Result<TOut>.Fail(ErrorMessage!);
    }

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
    {
        return IsSuccess ? bind(Value!) : Result<TOut>.Fail(ErrorMessage!);
    }

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string, TOut> onError)
    {
        return IsSuccess ? onSuccess(Value!) : onError(ErrorMessage!);
    }
}

public static class Result
{
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Fail<T>(string error) => Result<T>.Fail(error);
}
