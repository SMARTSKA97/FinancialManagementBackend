using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Application.DTOs.TransactionCategory;
using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinancialPlanner.Application.Services;

public class TransactionCategoryService : ICategoryService<TransactionCategoryDto, UpsertTransactionCategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TransactionCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IReadOnlyList<TransactionCategoryDto>>> GetAllAsync(string userId)
    {
        var allCategories = await _unitOfWork.TransactionCategoryRepository.FindAsync(c => c.UserId == userId);
        var mapped = _mapper.Map<IReadOnlyList<TransactionCategoryDto>>(allCategories);
        return ApiResponse<IReadOnlyList<TransactionCategoryDto>>.Success(mapped);
    }

    public async Task<ApiResponse<TransactionCategoryDto>> UpsertAsync(string userId, UpsertTransactionCategoryDto dto)
    {
        TransactionCategory category;
        if (dto.Id.HasValue && dto.Id > 0)
        {
            var existingCategory = await _unitOfWork.TransactionCategoryRepository.GetByIdAsync(dto.Id.Value);
            if (existingCategory == null || existingCategory.UserId != userId)
                return ApiResponse<TransactionCategoryDto>.Failure("Category not found.");
            category = existingCategory;
            _mapper.Map(dto, category);
            _unitOfWork.TransactionCategoryRepository.Upsert(category);
        }
        else
        {
            category = _mapper.Map<TransactionCategory>(dto);
            category.UserId = userId;
            _unitOfWork.TransactionCategoryRepository.Upsert(category);
        }

        try
        {
            await _unitOfWork.CompleteAsync();
            var resultDto = _mapper.Map<TransactionCategoryDto>(category);
            return ApiResponse<TransactionCategoryDto>.Success(resultDto);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
        {
            return ApiResponse<TransactionCategoryDto>.Failure("A category with this name already exists.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string userId, int id)
    {
        var category = await _unitOfWork.TransactionCategoryRepository.GetByIdAsync(id);
        if (category == null || category.UserId != userId)
            return ApiResponse<bool>.Failure("Category not found.");

        _unitOfWork.TransactionCategoryRepository.Delete(category);
        await _unitOfWork.CompleteAsync();
        return ApiResponse<bool>.Success(true, "Category deleted successfully.");
    }
}