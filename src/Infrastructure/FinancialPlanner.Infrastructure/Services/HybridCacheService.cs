using FinancialPlanner.Application.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace FinancialPlanner.Infrastructure.Services;

public class HybridCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _redisDb;
    private readonly ILogger<HybridCacheService> _logger;
    
    // Default L1 (Memory) TTL: 1 minute (protects Redis from bursts)
    private readonly TimeSpan _defaultL1Expiry = TimeSpan.FromMinutes(1);
    
    // Default L2 (Redis) TTL: 10 minutes (shared state)
    private readonly TimeSpan _defaultL2Expiry = TimeSpan.FromMinutes(10);

    public HybridCacheService(
        IMemoryCache memoryCache, 
        IConnectionMultiplexer redis, 
        ILogger<HybridCacheService> logger)
    {
        _memoryCache = memoryCache;
        _redis = redis;
        _redisDb = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        // 1. Check L1 (Memory) - Cost: 0 Redis requests
        if (_memoryCache.TryGetValue(key, out T? memoryValue))
        {
            // _logger.LogDebug("[Cache] L1 Hit (Memory): {Key}", key); // Verbose
            return memoryValue;
        }

        // 2. Check L2 (Redis) - Cost: 1 Redis request
        try
        {
            var redisValue = await _redisDb.StringGetAsync(key);
            if (!redisValue.IsNullOrEmpty)
            {
                var value = JsonSerializer.Deserialize<T>(redisValue.ToString());
                
                // Populate L1 for next time (Short TTL)
                if (value != null)
                {
                    _memoryCache.Set(key, value, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _defaultL1Expiry,
                        Size = 1 // Required when SizeLimit is set
                    });
                    _logger.LogDebug("[Cache] L2 Hit (Redis) -> Saved to L1: {Key}", key);
                }
                
                return value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cache] Redis error reading {Key}", key);
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var ttl = expiry ?? _defaultL2Expiry;

        // 1. Set L1 (Memory)
        _memoryCache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _defaultL1Expiry,
            Size = 1 // Required when SizeLimit is set
        });

        // 2. Set L2 (Redis)
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _redisDb.StringSetAsync(key, json, ttl);
            _logger.LogDebug("[Cache] Set L1 & L2: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cache] Redis error writing {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        // 1. Remove L1
        _memoryCache.Remove(key);

        // 2. Remove L2
        try
        {
            await _redisDb.KeyDeleteAsync(key);
            
            // TODO: In a multi-server setup, we should publish a Redis message here 
            // to tell other servers to clear their L1 cache for this key.
            // For now, we rely on the short L1 TTL (1 min) for eventual consistency.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cache] Redis error deleting {Key}", key);
        }
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        // 1. Try Get
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null && !EqualityComparer<T>.Default.Equals(cachedValue, default(T)))
        {
            return cachedValue;
        }

        // 2. Execute Factory (DB Call)
        var value = await factory();

        // 3. Set Cache
        if (value != null)
        {
            await SetAsync(key, value, expiry);
        }

        return value;
    }
}
