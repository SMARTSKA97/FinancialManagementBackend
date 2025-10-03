using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.AccountCategory;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Services;

public class AccountCategoryService : ICategoryService<AccountCategoryDto, UpsertAccountCategoryDto>
{
    private readonly IGenericRepository<AccountCategory> _repository;
    private readonly IMapper _mapper;

    public AccountCategoryService(IGenericRepository<AccountCategory> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IReadOnlyList<AccountCategoryDto>>> GetAllAsync(string userId)
    {
        var categories = await _repository.FindAsync(c => c.UserId == userId);
        var categoryDtos = _mapper.Map<IReadOnlyList<AccountCategoryDto>>(categories);
        return ApiResponse<IReadOnlyList<AccountCategoryDto>>.Success(categoryDtos);
    }

    public async Task<ApiResponse<AccountCategoryDto>> UpsertAsync(string userId, UpsertAccountCategoryDto dto)
    {
        AccountCategory category;
        if (dto.Id.HasValue && dto.Id > 0)
        {
            category = await _repository.GetByIdAsync(dto.Id.Value);
            if (category == null || category.UserId != userId)
            {
                return ApiResponse<AccountCategoryDto>.Failure("Category not found.");
            }
            _mapper.Map(dto, category);
        }
        else
        {
            category = _mapper.Map<AccountCategory>(dto);
            category.UserId = userId;
        }

        var savedCategory = await _repository.UpsertAsync(category);
        var resultDto = _mapper.Map<AccountCategoryDto>(savedCategory);
        return ApiResponse<AccountCategoryDto>.Success(resultDto);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string userId, int id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null || category.UserId != userId)
        {
            return ApiResponse<bool>.Failure("Category not found.");
        }

        await _repository.DeleteAsync(category);
        return ApiResponse<bool>.Success(true, "Category deleted successfully.");
    }
}