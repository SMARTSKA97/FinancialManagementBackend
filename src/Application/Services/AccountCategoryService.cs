using AutoMapper;
using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.AccountCategory;
using Application.DTOs.Categories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Application.Services;

public class AccountCategoryService : ICategoryService<AccountCategoryDto, UpsertAccountCategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    // Cache key helpers — short prefixes save Upstash storage bytes
    private static string AllKey(string userId) => $"ac:all:{userId}";

    public AccountCategoryService(IApplicationDbContext context, IMapper mapper, ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<Result<PaginatedResult<AccountCategoryDto>>> GetPagedAsync(string userId, QueryParameters queryParams)
    {
        var query = _context.AccountCategories
            .Where(c => c.UserId == userId && !c.IsDeleted)
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

        var mapped = _mapper.Map<List<AccountCategoryDto>>(items);
        var result = new PaginatedResult<AccountCategoryDto>(mapped, totalRecords, queryParams.PageNumber, queryParams.PageSize);
        return Result.Success(result);
    }

    public async Task<Result<IReadOnlyList<AccountCategoryDto>>> GetAllAsync(string userId)
    {
        var cacheKey = AllKey(userId);
        var cached = await _cache.GetAsync<IReadOnlyList<AccountCategoryDto>>(cacheKey);
        if (cached is not null)
            return Result.Success(cached);

        var allCategories = await _context.AccountCategories
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync();

        var mapped = _mapper.Map<IReadOnlyList<AccountCategoryDto>>(allCategories);
        await _cache.SetAsync(cacheKey, mapped, TimeSpan.FromHours(1));
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
            await _cache.RemoveAsync(AllKey(userId)); // invalidate stale list
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

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        _context.AccountCategories.Update(category);
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync(AllKey(userId));
        return Result.Success(true);
    }
}