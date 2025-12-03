using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Auth; // Added for RefreshTokenData

namespace FinancialPlanner.Application.Contracts;

public interface IRedisService
{
    // Refresh Token Operations
    Task<bool> SetRefreshTokenAsync(string token, RefreshTokenData data, TimeSpan expiry);
    Task<RefreshTokenData?> GetRefreshTokenAsync(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
    Task<long> RevokeRefreshTokensAsync(IEnumerable<string> tokens); // ⭐ Batch Revocation
    
    /// <summary>
    /// Atomically revokes an old token and sets a new one in a single Redis round-trip.
    /// </summary>
    Task<bool> RotateRefreshTokenAsync(string oldToken, string newToken, RefreshTokenData newData, TimeSpan expiry);

    // Health Check
    Task<bool> PingAsync();

    // General Cache Operations
    Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> GetStringAsync(string key);
    Task<bool> DeleteKeyAsync(string key);
}
