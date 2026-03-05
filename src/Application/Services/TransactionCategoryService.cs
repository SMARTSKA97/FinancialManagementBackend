using AutoMapper;
using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Categories;
using Application.DTOs.TransactionCategory;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Application.Services;

public class TransactionCategoryService : ICategoryService<TransactionCategoryDto, UpsertTransactionCategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public TransactionCategoryService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<TransactionCategoryDto>>> GetPagedAsync(string userId, QueryParameters queryParams)
    {
        var query = _context.TransactionCategories
            .Where(c => c.UserId == userId)
            .AsQueryable();

        // Apply global search
        if (!string.IsNullOrWhiteSpace(queryParams.GlobalSearch))
        {
            query = query.Where(c => c.Name.ToLower().Contains(queryParams.GlobalSearch.ToLower()));
        }

        // Get total count before pagination
        var totalRecords = await query.CountAsync();

        // Apply sorting
        query = queryParams.SortBy?.ToLower() switch
        {
            "name" => queryParams.SortOrder == "desc" ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            _ => query.OrderBy(c => c.Name)
        };

        // Apply pagination
        var items = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var mapped = _mapper.Map<List<TransactionCategoryDto>>(items);
        var result = new PaginatedResult<TransactionCategoryDto>(mapped, totalRecords, queryParams.PageNumber, queryParams.PageSize);
        return Result.Success(result);
    }

    public async Task<Result<IReadOnlyList<TransactionCategoryDto>>> GetAllAsync(string userId)
    {
        var allCategories = await _context.TransactionCategories
            .Where(c => c.UserId == userId)
            .ToListAsync();
        
        var mapped = _mapper.Map<IReadOnlyList<TransactionCategoryDto>>(allCategories);
        return Result.Success(mapped);
    }

    public async Task<Result<TransactionCategoryDto>> UpsertAsync(string userId, UpsertTransactionCategoryDto dto)
    {
        TransactionCategory? category = null;

        if (dto.Id.HasValue && dto.Id > 0)
        {
            category = await _context.TransactionCategories.FindAsync(dto.Id.Value);

            if (category == null || category.UserId != userId)
                return Result.Failure<TransactionCategoryDto>(new Error("Category.NotFound", "Category not found."));

            _mapper.Map(dto, category);
            _context.TransactionCategories.Update(category);
        }
        else
        {
            category = _mapper.Map<TransactionCategory>(dto);
            category.UserId = userId;
            _context.TransactionCategories.Add(category);
        }

        try
        {
            await _context.SaveChangesAsync();
            var resultDto = _mapper.Map<TransactionCategoryDto>(category);
            return Result.Success(resultDto);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
        {
            return Result.Failure<TransactionCategoryDto>(new Error("Category.Duplicate", "A category with this name already exists."));
        }
    }

    public async Task<Result<bool>> DeleteAsync(string userId, int id)
    {
        var category = await _context.TransactionCategories.FindAsync(id);
        if (category == null || category.UserId != userId)
            return Result.Failure<bool>(new Error("Category.NotFound", "Category not found."));

        _context.TransactionCategories.Remove(category);
        await _context.SaveChangesAsync();
        return Result.Success(true);
    }
}