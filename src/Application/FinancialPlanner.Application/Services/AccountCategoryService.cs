using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.AccountCategory;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinancialPlanner.Application.Services;

public class AccountCategoryService : ICategoryService<AccountCategoryDto, UpsertAccountCategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AccountCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IReadOnlyList<AccountCategoryDto>>> GetAllAsync(string userId)
    {
        var allCategories = await _unitOfWork.AccountCategoryRepository.FindAsync(c => c.UserId == userId);
        var mapped = _mapper.Map<IReadOnlyList<AccountCategoryDto>>(allCategories);
        return ApiResponse<IReadOnlyList<AccountCategoryDto>>.Success(mapped);
    }

    public async Task<ApiResponse<AccountCategoryDto>> UpsertAsync(string userId, UpsertAccountCategoryDto dto)
    {
        AccountCategory category;
        if (dto.Id.HasValue && dto.Id > 0)
        {
            var existingCategory = await _unitOfWork.AccountCategoryRepository.GetByIdAsync(dto.Id.Value);
            if (existingCategory == null || existingCategory.UserId != userId)
                return ApiResponse<AccountCategoryDto>.Failure("Category not found.");
            category = existingCategory;
            category.Name = dto.Name;
            _unitOfWork.AccountCategoryRepository.Upsert(category);
        }
        else
        {
            category = _mapper.Map<AccountCategory>(dto);
            category.UserId = userId;
            _unitOfWork.AccountCategoryRepository.Upsert(category);
        }

        try
        {
            await _unitOfWork.CompleteAsync();
            var resultDto = _mapper.Map<AccountCategoryDto>(category);
            return ApiResponse<AccountCategoryDto>.Success(resultDto);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
        {
            return ApiResponse<AccountCategoryDto>.Failure("A category with this name already exists.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string userId, int id)
    {
        var category = await _unitOfWork.AccountCategoryRepository.GetByIdAsync(id);
        if (category == null || category.UserId != userId)
            return ApiResponse<bool>.Failure("Category not found.");

        _unitOfWork.AccountCategoryRepository.Delete(category);
        await _unitOfWork.CompleteAsync();
        return ApiResponse<bool>.Success(true, "Category deleted successfully.");
    }
}