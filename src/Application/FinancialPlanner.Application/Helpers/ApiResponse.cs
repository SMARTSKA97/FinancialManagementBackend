namespace FinancialPlanner.Application;

// This enum gives us a clear, readable status instead of magic numbers like 1, 2, 3
public enum ApiResponseStatus
{
    Success,
    Warning,
    Error
}

public class ApiResponse<T>
{
    public T? Result { get; private set; }
    public ApiResponseStatus ApiResponseStatus { get; private set; }
    public string Message { get; private set; }
    public List<string>? Errors { get; private set; }

    private ApiResponse(ApiResponseStatus status, string message, T? result, List<string>? errors = null)
    {
        ApiResponseStatus = status;
        Message = message;
        Result = result;
        Errors = errors;
    }

    public static ApiResponse<T> Success(T result, string message = "Request completed successfully.")
    {
        return new ApiResponse<T>(ApiResponseStatus.Success, message, result);
    }

    public static ApiResponse<T> Failure(string errorMessage, List<string>? validationErrors = null)
    {
        return new ApiResponse<T>(ApiResponseStatus.Error, errorMessage, default, validationErrors);
    }

    public static ApiResponse<T> Warning(string warningMessage, T? result = default)
    {
        return new ApiResponse<T>(ApiResponseStatus.Warning, warningMessage, result);
    }
}