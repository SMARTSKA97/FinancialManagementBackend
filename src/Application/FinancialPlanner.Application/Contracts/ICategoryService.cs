using FinancialPlanner.Application.Common.Models;
using FinancialPlanner.Application.DTOs.Categories;

namespace FinancialPlanner.Application.Contracts;

public interface ICategoryService<TDto, TUpsertDto>
{
    Task<Result<IReadOnlyList<TDto>>> GetAllAsync(string userId);
    Task<Result<TDto>> UpsertAsync(string userId, TUpsertDto dto);
    Task<Result<bool>> DeleteAsync(string userId, int id);
}