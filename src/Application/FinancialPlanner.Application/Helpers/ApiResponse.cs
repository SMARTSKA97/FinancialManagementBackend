namespace FinancialPlanner.Application;

public enum ApiResponseStatus
{
    Success,
    Warning,
    Error
}

public class ApiResponse<T>
{
    public T? Result { get; set; }
    public ApiResponseStatus ApiResponseStatus { get; set; }
    public string Message { get; set; }
    public List<string>? Errors { get; set; }
    public bool IsSuccess => ApiResponseStatus == ApiResponseStatus.Success;

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