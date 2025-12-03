namespace FinancialPlanner.Application.Contracts;

public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache (L1 Memory -> L2 Redis).
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets a value in cache (L1 Memory + L2 Redis).
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>
    /// Removes a value from cache (L1 Memory + L2 Redis).
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Gets a value from cache, or executes the factory function to get it and then caches it.
    /// This is the "Lazy Loading" pattern.
    /// </summary>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
}
