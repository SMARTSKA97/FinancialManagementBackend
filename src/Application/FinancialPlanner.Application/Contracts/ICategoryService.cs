using FinancialPlanner.Application.DTOs.Categories;

namespace FinancialPlanner.Application.Contracts;

public interface ICategoryService<TDto, TUpsertDto>
{
    Task<ApiResponse<IReadOnlyList<TDto>>> GetAllAsync(string userId);
    Task<ApiResponse<TDto>> UpsertAsync(string userId, TUpsertDto dto);
    Task<ApiResponse<bool>> DeleteAsync(string userId, int id);
}