using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Auth; // Added for RefreshTokenData
using FinancialPlanner.Application.DTOs.Auth;
using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FinancialPlanner.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly IRedisService? _redisService;

    public AuthService(
        UserManager<ApplicationUser> userManager, 
        IOptions<JwtSettings> jwtOptions, 
        ILogger<AuthService> logger,
        IRedisService? redisService = null)
    {
        _userManager = userManager;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
        _redisService = redisService;
    }

    public async Task<ApiResponse<string>> RegisterAsync(RegisterUserDto dto)
    {
        var user = new ApplicationUser
        {
            Name = dto.Name,
            DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc),
            Email = dto.Email,
            UserName = dto.UserName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("User registration failed for {Email}. Errors: {Errors}", dto.Email, string.Join(", ", errors));
            return ApiResponse<string>.Failure("User registration failed.", errors);
        }

        _logger.LogInformation("User registered successfully: {Email}", dto.Email);
        return ApiResponse<string>.Success("User registered successfully.");
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginUserDto dto, string ipAddress, string userAgent)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens) // Important: Load existing tokens
            .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            _logger.LogWarning("Failed login attempt for {UserName} from IP {IpAddress}", dto.UserName, ipAddress);
            return ApiResponse<LoginResponseDto>.Failure("Invalid username or password.");
        }

        // Concurrent Login Check
        if (!string.IsNullOrEmpty(user.CurrentSessionId) && !dto.ForceLogin)
        {
            _logger.LogWarning("Concurrent login attempt for {UserName} from IP {IpAddress}. Existing session active.", dto.UserName, ipAddress);
            return ApiResponse<LoginResponseDto>.Failure("User is already logged in on another device. Do you want to continue here?", new List<string> { "ConcurrentLogin" });
        }

        // Start New Session
        var sessionId = Guid.NewGuid().ToString();

        // Concurrent Login Cleanup: If forcing login, remove the token from the previous session (LastKnownIp)
        if (dto.ForceLogin && !string.IsNullOrEmpty(user.LastKnownIp))
        {
             _logger.LogInformation("Forcing login for {UserName}. Revoking tokens from previous IP {LastKnownIp}", dto.UserName, user.LastKnownIp);
             
             // ⭐ Revoke old tokens from Redis too
             if (_redisService != null)
             {
                 var oldTokens = user.RefreshTokens
                    .Where(rt => rt.CreatedByIp == user.LastKnownIp)
                    .Select(rt => rt.Token)
                    .ToList();
                 
                 if (oldTokens.Any())
                 {
                     await _redisService.RevokeRefreshTokensAsync(oldTokens);
                     _logger.LogInformation("[Redis] 🗑️ Force login - batch revoked {Count} old tokens", oldTokens.Count);
                 }
             }
             
             user.RefreshTokens.RemoveAll(rt => rt.CreatedByIp == user.LastKnownIp);
        }

        user.CurrentSessionId = sessionId;
        user.LastLoginTime = DateTime.UtcNow;
        user.LastKnownIp = ipAddress;
        user.LastKnownUserAgent = userAgent;

        var accessToken = GenerateJwtToken(user, sessionId);
        var refreshToken = GenerateRefreshToken(ipAddress);

        // Enforce One Token Per IP: Remove any existing active tokens for this IP
        // ⭐ Clean up from Redis first
        if (_redisService != null)
        {
            var currentIpTokens = user.RefreshTokens
                .Where(rt => rt.CreatedByIp == ipAddress)
                .Select(rt => rt.Token)
                .ToList();

            if (currentIpTokens.Any())
            {
                await _redisService.RevokeRefreshTokensAsync(currentIpTokens);
                _logger.LogInformation("[Redis] 🗑️ Same-IP batch revoked {Count} tokens", currentIpTokens.Count);
            }
        }
        user.RefreshTokens.RemoveAll(rt => rt.CreatedByIp == ipAddress);

        // Aggressive Cleanup: Remove ALL tokens that are expired OR revoked
        user.RefreshTokens.RemoveAll(rt => !rt.IsActive || rt.ExpiresUtc <= DateTime.UtcNow);

        // ⭐ DUAL-WRITE: Store in Redis (primary) + Database (backup during migration)
        if (_redisService != null)
        {
            await _redisService.SetRefreshTokenAsync(
                refreshToken.Token,
                new RefreshTokenData
                {
                    UserId = user.Id,
                    Token = refreshToken.Token,
                    CreatedByIp = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = refreshToken.CreatedUtc,
                    ExpiresUtc = refreshToken.ExpiresUtc
                },
                TimeSpan.FromDays(_jwtSettings.RefreshTokenTTL)
            );
            _logger.LogInformation("[Redis] ✅ Token stored for {UserName}", user.UserName);
        }

        user.RefreshTokens.Add(refreshToken);
        var updateResult = await _userManager.UpdateAsync(user);
        
        if (!updateResult.Succeeded)
        {
            _logger.LogError("Login failed for {UserName} during DB update. Errors: {Errors}", dto.UserName, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            return ApiResponse<LoginResponseDto>.Failure("Login failed.", updateResult.Errors.Select(e => e.Description).ToList());
        }

        _logger.LogInformation("User {UserName} logged in successfully from IP {IpAddress}", dto.UserName, ipAddress);
        return ApiResponse<LoginResponseDto>.Success(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            UserName = user.UserName!
        });
    }

    public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string token, string ipAddress)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
        {
            _logger.LogWarning("Refresh token attempt failed. Token not found or user does not exist. IP: {IpAddress}", ipAddress);
            return ApiResponse<LoginResponseDto>.Failure("Invalid token.");
        }

        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
        {
            _logger.LogWarning("Refresh token attempt failed. Token is inactive/revoked. User: {UserName}, IP: {IpAddress}", user.UserName, ipAddress);
            return ApiResponse<LoginResponseDto>.Failure("Invalid token.");
        }

        // Revoke the old token (Rotation)
        refreshToken.RevokedUtc = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;

        // Generate new tokens
        var newAccessToken = GenerateJwtToken(user, user.CurrentSessionId ?? "");
        var newRefreshToken = GenerateRefreshToken(ipAddress);

        // Enforce One Token Per IP (for the new token)
        user.RefreshTokens.RemoveAll(rt => rt.CreatedByIp == ipAddress);

        // Aggressive Cleanup
        user.RefreshTokens.RemoveAll(rt => !rt.IsActive || rt.ExpiresUtc <= DateTime.UtcNow);

        // ⭐ DUAL-WRITE: Revoke old + Store new in Redis - OPTIMIZED ROTATION
        if (_redisService != null)
        {
            await _redisService.RotateRefreshTokenAsync(token, newRefreshToken.Token,
                new RefreshTokenData
                {
                    UserId = user.Id,
                    Token = newRefreshToken.Token,
                    CreatedByIp = ipAddress,
                    UserAgent = user.LastKnownUserAgent ?? "Unknown",
                    CreatedAt = newRefreshToken.CreatedUtc,
                    ExpiresUtc = newRefreshToken.ExpiresUtc
                },
                TimeSpan.FromDays(_jwtSettings.RefreshTokenTTL)
            );
            _logger.LogInformation("[Redis] ✅ Token rotated for {UserName}", user.UserName);
        }

        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Token refreshed successfully for user {UserName} from IP {IpAddress}", user.UserName, ipAddress);

        return ApiResponse<LoginResponseDto>.Success(new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            UserName = user.UserName!
        });
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string token, string ipAddress)
    {
        // Find the user who owns this token
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        if (user == null) return ApiResponse<bool>.Success(true); // Already logged out effectively

        var refreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == token);
        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.RevokedUtc = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            
            // ⭐ CRITICAL: Clear session state to fully terminate unattended sessions
            user.CurrentSessionId = null;
            user.LastKnownIp = null;
            user.LastKnownUserAgent = null;
            
            // ⭐ Revoke in Redis too
            if (_redisService != null)
            {
                await _redisService.RevokeRefreshTokenAsync(token);
                _logger.LogInformation("[Redis] ✅ Token revoked for {UserName}", user.UserName);
            }
            
            await _userManager.UpdateAsync(user);
            _logger.LogInformation("Session terminated for {UserName} from IP {IpAddress}", user.UserName, ipAddress);
        }

        return ApiResponse<bool>.Success(true, "Logged out successfully.");
    }

    private string GenerateJwtToken(ApplicationUser user, string sessionId)
    {
        var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("SessionId", sessionId) // Add SessionId claim
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RefreshToken GenerateRefreshToken(string ipAddress)
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            ExpiresUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenTTL), // Long life
            CreatedUtc = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }
}