using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.TransactionCategory;
using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        var allCategories = await _repository.FindAsync(c => c.UserId == userId);
        var mapped = _mapper.Map<IReadOnlyList<TransactionCategoryDto>>(allCategories);
        return ApiResponse<IReadOnlyList<TransactionCategoryDto>>.Success(mapped);
    }

    public async Task<ApiResponse<TransactionCategoryDto>> UpsertAsync(string userId, UpsertTransactionCategoryDto dto)
    {
        TransactionCategory category;
        if (dto.Id.HasValue && dto.Id > 0)
        {
            var existingCategory = await _repository.GetByIdAsync(dto.Id.Value);
            if (existingCategory == null || existingCategory.UserId != userId)
                return ApiResponse<TransactionCategoryDto>.Failure("Category not found.");
            category = existingCategory;
            category.Name = dto.Name;
        }
        else
        {
            category = new TransactionCategory { Name = dto.Name, UserId = userId };
        }

        try
        {
            var savedCategory = await _repository.UpsertAsync(category);
            var resultDto = _mapper.Map<TransactionCategoryDto>(savedCategory);
            return ApiResponse<TransactionCategoryDto>.Success(resultDto);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
        {
            // This catches the specific "unique violation" error
            return ApiResponse<TransactionCategoryDto>.Failure("A category with this name already exists.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string userId, int id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null || category.UserId != userId)
            return ApiResponse<bool>.Failure("Category not found.");

        await _repository.DeleteAsync(category);
        return ApiResponse<bool>.Success(true, "Category deleted successfully.");
    }
}

