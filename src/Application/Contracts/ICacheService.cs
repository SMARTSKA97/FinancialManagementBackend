namespace Application.Contracts;

/// <summary>
/// Two-tier cache abstraction:
///   L1 = IMemoryCache (in-process, zero latency, free)
///   L2 = Redis / Upstash (distributed, persists across restarts)
///
/// Design goal: minimise Upstash command usage.
/// Most reads are served from L1 (memory) — Redis is only hit on L1 miss.
/// Writes go to both tiers atomically.
/// </summary>
public interface ICacheService
{
    /// <summary>Returns cached value, checking L1 first then L2.</summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>Sets value in both L1 and L2. L1 uses the same or shorter TTL.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan expiry);

    /// <summary>Removes key from both tiers.</summary>
    Task RemoveAsync(string key);

    /// <summary>Pattern-delete from Redis only. L1 entries expire naturally.</summary>
    Task RemoveByPrefixAsync(string prefix);
}
