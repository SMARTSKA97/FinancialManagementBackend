using FinancialPlanner.Application.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FinancialPlanner.Infrastructure.Services;

public class RedisRateLimiter : IRedisRateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly IMemoryCache _memoryCache; // ⭐ L1 Cache for Blocks
    private readonly ILogger<RedisRateLimiter> _logger;

    public RedisRateLimiter(
        IConnectionMultiplexer redis, 
        IMemoryCache memoryCache,
        ILogger<RedisRateLimiter> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<RateLimitResult> CheckLimitAsync(string clientId, string endpoint, int maxRequests, TimeSpan window)
    {
        try
        {
            var key = GetKey(clientId, endpoint);

            // 1. Check L1 Block Cache (Cost: 0 Redis)
            if (_memoryCache.TryGetValue($"block:{key}", out _))
            {
                _logger.LogWarning("[RateLimit] ⛔ L1 Blocked {ClientId}. Saving Redis calls.", clientId);
                return new RateLimitResult
                {
                    IsAllowed = false,
                    Remaining = 0,
                    ResetTime = window, // Approximate
                    Limit = maxRequests
                };
            }

            // 2. Fixed Window Algorithm (Cost: 1 Redis Command - INCR)
            // Lua script:
            // - INCR key
            // - If == 1 (new window), EXPIRE key window
            // - Return count, ttl
            var script = @"
                local count = redis.call('INCR', KEYS[1])
                local ttl = redis.call('TTL', KEYS[1])
                if count == 1 then
                    redis.call('EXPIRE', KEYS[1], ARGV[1])
                    ttl = tonumber(ARGV[1])
                end
                return {count, ttl}
            ";

            var result = (RedisResult[])await _db.ScriptEvaluateAsync(script, 
                new RedisKey[] { key }, 
                new RedisValue[] { (long)window.TotalSeconds });

            var count = (int)result[0];
            var ttl = (int)result[1];
            var remaining = Math.Max(0, maxRequests - count);
            var isAllowed = count <= maxRequests;

            if (isAllowed)
            {
                _logger.LogInformation("[RateLimit] Allowed {ClientId}. Count: {Count}/{Max}", clientId, count, maxRequests);
            }
            else
            {
                _logger.LogWarning("[RateLimit] ⛔ Blocked {ClientId}. Limit reached.", clientId);
                
                // 3. Set L1 Block Cache (Cost: 0 Redis)
                // Cache locally for the remaining window time so we don't hit Redis again
                if (ttl > 0)
                {
                    _memoryCache.Set($"block:{key}", true, TimeSpan.FromSeconds(ttl));
                }
            }

            return new RateLimitResult
            {
                IsAllowed = isAllowed,
                Remaining = remaining,
                ResetTime = TimeSpan.FromSeconds(ttl),
                Limit = maxRequests
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RateLimit] Error checking limit. Fail-open.");
            return new RateLimitResult 
            { 
                IsAllowed = true, 
                Remaining = 1, 
                ResetTime = TimeSpan.Zero,
                Limit = maxRequests
            };
        }
    }

    public async Task<bool> IsAllowedAsync(string clientId, string endpoint, int maxRequests, TimeSpan window)
    {
        var result = await CheckLimitAsync(clientId, endpoint, maxRequests, window);
        return result.IsAllowed;
    }

    public async Task<int> GetRemainingRequestsAsync(string clientId, string endpoint, int maxRequests, TimeSpan window)
    {
        try
        {
            var key = GetKey(clientId, endpoint);
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty) return maxRequests;
            
            return Math.Max(0, maxRequests - (int)value);
        }
        catch
        {
            return maxRequests;
        }
    }

    public async Task<TimeSpan> GetResetTimeAsync(string clientId, string endpoint, TimeSpan window)
    {
        try
        {
            var key = GetKey(clientId, endpoint);
            var ttl = await _db.KeyTimeToLiveAsync(key);
            return ttl ?? TimeSpan.Zero;
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }

    private static string GetKey(string clientId, string endpoint)
    {
        var cleanEndpoint = endpoint.Replace("/", "_").TrimStart('_');
        return $"ratelimit:{clientId}:{cleanEndpoint}";
    }
}
