using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Auth;
using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinancialPlanner.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;

    public AuthService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _jwtSettings = jwtOptions.Value;
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
            return ApiResponse<string>.Failure("User registration failed.", errors);
        }

        return ApiResponse<string>.Success("User registered successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginUserDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return ApiResponse<AuthResponseDto>.Failure("Invalid username or password.");
        }

        var token = GenerateJwtToken(user);
        var authResponse = new AuthResponseDto { Token = token };

        return ApiResponse<AuthResponseDto>.Success(authResponse);
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}