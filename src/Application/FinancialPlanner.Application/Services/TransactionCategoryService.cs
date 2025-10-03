using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Application.DTOs.TransactionCategory;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Services;

public class TransactionCategoryService : ICategoryService<TransactionCategoryDto, UpsertTransactionCategoryDto>
{
    private readonly IGenericRepository<TransactionCategory> _repository;
    private readonly IMapper _mapper;

    public TransactionCategoryService(IGenericRepository<TransactionCategory> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IReadOnlyList<TransactionCategoryDto>>> GetAllAsync(string userId)
    {
        var categories = await _repository.FindAsync(c => c.UserId == userId);
        var categoryDtos = _mapper.Map<IReadOnlyList<TransactionCategoryDto>>(categories);
        return ApiResponse<IReadOnlyList<TransactionCategoryDto>>.Success(categoryDtos);
    }

    public async Task<ApiResponse<TransactionCategoryDto>> UpsertAsync(string userId, UpsertTransactionCategoryDto dto)
    {
        TransactionCategory category;
        if (dto.Id.HasValue && dto.Id > 0)
        {
            category = await _repository.GetByIdAsync(dto.Id.Value);
            if (category == null || category.UserId != userId)
            {
                return ApiResponse<TransactionCategoryDto>.Failure("Category not found.");
            }
            _mapper.Map(dto, category);
        }
        else
        {
            category = _mapper.Map<TransactionCategory>(dto);
            category.UserId = userId;
        }

        var savedCategory = await _repository.UpsertAsync(category);
        var resultDto = _mapper.Map<TransactionCategoryDto>(savedCategory);
        return ApiResponse<TransactionCategoryDto>.Success(resultDto);
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
