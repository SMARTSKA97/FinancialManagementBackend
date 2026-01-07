using FinancialPlanner.Application.Common.Models;
using FinancialPlanner.Application.Contracts;
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

    public AuthService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<Result<string>> RegisterAsync(RegisterUserDto dto)
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
            return Result.Failure<string>(new Error("Auth.RegistrationFailed", $"User registration failed: {string.Join(", ", errors)}"));
        }

        _logger.LogInformation("User registered successfully: {Email}", dto.Email);
        return Result.Success("User registered successfully.");
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginUserDto dto, string ipAddress, string userAgent)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens) // Important: Load existing tokens
            .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            _logger.LogWarning("Failed login attempt for {UserName} from IP {IpAddress}", dto.UserName, ipAddress);
            return Result.Failure<LoginResponseDto>(new Error("Auth.InvalidCredentials", "Invalid username or password."));
        }

        // Concurrent Login Check
        if (!string.IsNullOrEmpty(user.CurrentSessionId) && !dto.ForceLogin)
        {
            _logger.LogWarning("Concurrent login attempt for {UserName} from IP {IpAddress}. Existing session active.", dto.UserName, ipAddress);
            return Result.Failure<LoginResponseDto>(new Error("Auth.ConcurrentLogin", "User is already logged in on another device. Do you want to continue here?"));
        }

        // Start New Session
        var sessionId = Guid.NewGuid().ToString();

        // Concurrent Login Cleanup: If forcing login, remove the token from the previous session (LastKnownIp)
        if (dto.ForceLogin && !string.IsNullOrEmpty(user.LastKnownIp))
        {
             _logger.LogInformation("Forcing login for {UserName}. Revoking tokens from previous IP {LastKnownIp}", dto.UserName, user.LastKnownIp);
             user.RefreshTokens.RemoveAll(rt => rt.CreatedByIp == user.LastKnownIp);
        }

        user.CurrentSessionId = sessionId;
        user.LastLoginTime = DateTime.UtcNow;
        user.LastKnownIp = ipAddress;
        user.LastKnownUserAgent = userAgent;

        var accessToken = GenerateJwtToken(user, sessionId);
        var refreshToken = GenerateRefreshToken(ipAddress);

        // Enforce One Token Per IP: Remove any existing active tokens for this IP
        user.RefreshTokens.RemoveAll(rt => rt.CreatedByIp == ipAddress);

        // Aggressive Cleanup: Remove ALL tokens that are expired OR revoked
        user.RefreshTokens.RemoveAll(rt => !rt.IsActive || rt.ExpiresUtc <= DateTime.UtcNow);

        user.RefreshTokens.Add(refreshToken);
        var updateResult = await _userManager.UpdateAsync(user);
        
        if (!updateResult.Succeeded)
        {
            _logger.LogError("Login failed for {UserName} during DB update. Errors: {Errors}", dto.UserName, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            return Result.Failure<LoginResponseDto>(new Error("Auth.UpdateFailed", $"Login failed: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}"));
        }

        _logger.LogInformation("User {UserName} logged in successfully from IP {IpAddress}", dto.UserName, ipAddress);
        return Result.Success(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            UserName = user.UserName!
        });
    }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(string token, string ipAddress)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
        {
            _logger.LogWarning("Refresh token attempt failed. Token not found or user does not exist. IP: {IpAddress}", ipAddress);
            return Result.Failure<LoginResponseDto>(new Error("Auth.InvalidToken", "Invalid token."));
        }

        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
        {
            _logger.LogWarning("Refresh token attempt failed. Token is inactive/revoked. User: {UserName}, IP: {IpAddress}", user.UserName, ipAddress);
            return Result.Failure<LoginResponseDto>(new Error("Auth.InvalidToken", "Invalid token."));
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

        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Token refreshed successfully for user {UserName} from IP {IpAddress}", user.UserName, ipAddress);

        return Result.Success(new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            UserName = user.UserName!
        });
    }

    public async Task<Result<bool>> LogoutAsync(string token, string ipAddress)
    {
        // Find the user who owns this token
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        if (user == null) return Result.Success(true); // Already logged out effectively

        var refreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == token);
        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.RevokedUtc = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            
            // ⭐ CRITICAL: Clear session state to fully terminate unattended sessions
            user.CurrentSessionId = null;
            user.LastKnownIp = null;
            user.LastKnownUserAgent = null;
            
            await _userManager.UpdateAsync(user);
            _logger.LogInformation("Session terminated for {UserName} from IP {IpAddress}", user.UserName, ipAddress);
        }

        return Result.Success(true);
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