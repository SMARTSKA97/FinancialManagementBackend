using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Auth; // Added for RefreshTokenData
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace FinancialPlanner.Infrastructure.Services;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    // Refresh Token Operations
    public async Task<bool> SetRefreshTokenAsync(string token, RefreshTokenData data, TimeSpan expiry)
    {
        try
        {
            var key = $"refresh_token:{token}";
            var value = JsonSerializer.Serialize(data);
            var result = await _db.StringSetAsync(key, value, expiry);
            
            if (result)
            {
                _logger.LogInformation("[Redis] Refresh token stored: {Key}, TTL: {Expiry}", key, expiry);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Redis] Failed to set refresh token");
            return false;
        }
    }

    public async Task<RefreshTokenData?> GetRefreshTokenAsync(string token)
    {
        try
        {
            var key = $"refresh_token:{token}";
            var value = await _db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                _logger.LogWarning("[Redis] Refresh token not found: {Key}", key);
                return null;
            }

            var data = JsonSerializer.Deserialize<RefreshTokenData>(value.ToString());
            _logger.LogInformation("[Redis] Refresh token retrieved: {Key}", key);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Redis] Failed to get refresh token");
            return null;
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        try
        {
            var key = $"refresh_token:{token}";
            var result = await _db.KeyDeleteAsync(key);
            
            if (result)
            {
                _logger.LogInformation("[Redis] Refresh token revoked: {Key}", key);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Redis] Failed to revoke refresh token");
            return false;
        }
    }

    public async Task<long> RevokeRefreshTokensAsync(IEnumerable<string> tokens)
    {
        try
        {
            var keys = tokens.Select(t => (RedisKey)$"refresh_token:{t}").ToArray();
            if (keys.Length == 0) return 0;
            
            var count = await _db.KeyDeleteAsync(keys); // ⭐ Single Redis Command
            _logger.LogInformation("[Redis] Batch revoked {Count} tokens", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Redis] Failed to batch revoke tokens");
            return 0;
        }
    }

    public async Task<bool> RotateRefreshTokenAsync(string oldToken, string newToken, RefreshTokenData newData, TimeSpan expiry)
    {
        try
        {
            var oldKey = $"refresh_token:{oldToken}";
            var newKey = $"refresh_token:{newToken}";
            var newValue = JsonSerializer.Serialize(newData);

            // Lua script to atomically delete old token and set new token
            // KEYS[1] = oldKey
            // KEYS[2] = newKey
            // ARGV[1] = newValue
            // ARGV[2] = expiry (seconds)
            var script = @"
                redis.call('UNLINK', KEYS[1])
                redis.call('SETEX', KEYS[2], ARGV[2], ARGV[1])
                return 1
            ";

            await _db.ScriptEvaluateAsync(script, 
                new RedisKey[] { oldKey, newKey }, 
                new RedisValue[] { newValue, (long)expiry.TotalSeconds });

            _logger.LogInformation("[Redis] Token rotated. Revoked: {OldKey}, Stored: {NewKey}", oldKey, newKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Redis] Failed to rotate refresh token");
            return false;
        }
    }

    // Health Check
    public async Task<bool> PingAsync()
    {
        try
        {
            var result = await _db.PingAsync();
            _logger.LogInformation("[Redis] Ping successful: {Latency}ms", result.TotalMilliseconds);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Redis] Ping failed");
            return false;
        }
    }

    // General Cache Operations
    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        try
        {
            return await _db.StringSetAsync(key, value, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Redis] Failed to set string key: {Key}", key);
            return false;
        }
    }

    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            return value.IsNullOrEmpty ? null : value.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Redis] Failed to get string key: {Key}", key);
            return null;
        }
    }

    public async Task<bool> DeleteKeyAsync(string key)
    {
        try
        {
            return await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Redis] Failed to delete key: {Key}", key);
            return false;
        }
    }
}
