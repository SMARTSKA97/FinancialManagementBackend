namespace FinancialPlanner.Application.Contracts;

public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int Remaining { get; set; }
    public TimeSpan ResetTime { get; set; }
    public int Limit { get; set; }
}

public interface IRedisRateLimiter
{
    /// <summary>
    /// Check rate limit and return detailed result in a single atomic operation
    /// </summary>
    Task<RateLimitResult> CheckLimitAsync(string clientId, string endpoint, int maxRequests, TimeSpan window);

    // Deprecated methods (keep for backward compatibility if needed, or remove)
    Task<bool> IsAllowedAsync(string clientId, string endpoint, int maxRequests, TimeSpan window);
    Task<int> GetRemainingRequestsAsync(string clientId, string endpoint, int maxRequests, TimeSpan window);
    Task<TimeSpan> GetResetTimeAsync(string clientId, string endpoint, TimeSpan window);
}
