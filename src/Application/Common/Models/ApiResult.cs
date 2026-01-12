namespace Application.Common.Models;

public record ApiResult<T>(T? Value, bool IsSuccess, Error Error)
{
    public static ApiResult<T> Success(T value) => new(value, true, Error.None);
    public static ApiResult<T> Failure(Error error) => new(default, false, error);
}
