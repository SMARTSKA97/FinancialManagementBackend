using AutoMapper;
using FinancialPlanner.Application.Common.Models;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.AccountCategory;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinancialPlanner.Application.Services;

public class AccountCategoryService : ICategoryService<AccountCategoryDto, UpsertAccountCategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AccountCategoryService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<AccountCategoryDto>>> GetAllAsync(string userId)
    {
        var allCategories = await _context.AccountCategories
            .Where(c => c.UserId == userId)
            .ToListAsync();
        
        var mapped = _mapper.Map<IReadOnlyList<AccountCategoryDto>>(allCategories);
        return Result.Success(mapped);
    }

    public async Task<Result<AccountCategoryDto>> UpsertAsync(string userId, UpsertAccountCategoryDto dto)
    {
        AccountCategory? category = null;

        if (dto.Id.HasValue && dto.Id > 0)
        {
            category = await _context.AccountCategories.FindAsync(dto.Id.Value);

            if (category == null || category.UserId != userId)
                return Result.Failure<AccountCategoryDto>(new Error("Category.NotFound", "Category not found."));

            category.Name = dto.Name;
            _context.AccountCategories.Update(category);
        }
        else
        {
            category = _mapper.Map<AccountCategory>(dto);
            category.UserId = userId;
            _context.AccountCategories.Add(category);
        }

        try
        {
            await _context.SaveChangesAsync();
            var resultDto = _mapper.Map<AccountCategoryDto>(category);
            return Result.Success(resultDto);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
        {
            return Result.Failure<AccountCategoryDto>(new Error("Category.Duplicate", "A category with this name already exists."));
        }
    }

    public async Task<Result<bool>> DeleteAsync(string userId, int id)
    {
        var category = await _context.AccountCategories.FindAsync(id);
        if (category == null || category.UserId != userId)
            return Result.Failure<bool>(new Error("Category.NotFound", "Category not found."));

        _context.AccountCategories.Remove(category);
        await _context.SaveChangesAsync();
        return Result.Success(true);
    }
}