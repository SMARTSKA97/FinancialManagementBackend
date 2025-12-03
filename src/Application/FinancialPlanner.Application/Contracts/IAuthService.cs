using FinancialPlanner.Application.DTOs.Auth;

namespace FinancialPlanner.Application.Contracts;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginUserDto dto, string ipAddress, string userAgent);
    Task<ApiResponse<string>> RegisterAsync(RegisterUserDto dto);
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<ApiResponse<bool>> LogoutAsync(string refreshToken, string ipAddress);
}