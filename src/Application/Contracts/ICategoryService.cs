using Application.Common.Models;
using Application.DTOs.Categories;

namespace Application.Contracts;

public interface ICategoryService<TDto, TUpsertDto>
{
    Task<Result<PaginatedResult<TDto>>> GetPagedAsync(string userId, QueryParameters queryParams);
    Task<Result<IReadOnlyList<TDto>>> GetAllAsync(string userId);
    Task<Result<TDto>> UpsertAsync(string userId, TUpsertDto dto);
    Task<Result<bool>> DeleteAsync(string userId, int id);
}