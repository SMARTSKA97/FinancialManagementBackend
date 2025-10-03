using FinancialPlanner.Application.DTOs.Auth;

namespace FinancialPlanner.Application.Contracts;

public interface IAuthService
{
    Task<ApiResponse<string>> RegisterAsync(RegisterUserDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginUserDto dto);
}