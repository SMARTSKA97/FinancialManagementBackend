using FinancialPlanner.Application.Common.Models;
using FinancialPlanner.Application.DTOs.Auth;

namespace FinancialPlanner.Application.Contracts;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginUserDto dto, string ipAddress, string userAgent);
    Task<Result<string>> RegisterAsync(RegisterUserDto dto);
    Task<Result<LoginResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<Result<bool>> LogoutAsync(string refreshToken, string ipAddress);
}