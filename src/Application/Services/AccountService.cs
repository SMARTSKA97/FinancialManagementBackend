using AutoMapper;
using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Accounts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AccountService : IAccountService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AccountService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<AccountDto>>> GetPagedAccountsAsync(string userId, QueryParameters queryParams)
    {
        var queryable = _context.Accounts
            .Where(a => a.UserId == userId)
            .Include(a => a.AccountCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.GlobalSearch))
        {
            var search = queryParams.GlobalSearch.Trim();
            queryable = queryable.Where(a =>
                a.Name.Contains(search) ||
                (a.AccountCategory != null && a.AccountCategory.Name.Contains(search)));
        }

        var totalRecords = await queryable.CountAsync();

        var items = await queryable
            .OrderBy(a => a.Name)
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var pagedDto = new PaginatedResult<AccountDto>(
            _mapper.Map<List<AccountDto>>(items),
            totalRecords,
            queryParams.PageNumber,
            queryParams.PageSize
        );
        return Result.Success(pagedDto);
    }

    public async Task<Result<AccountDto>> UpsertAccountAsync(string userId, UpsertAccountDto dto)
    {
        Account? account = null;

        if (dto.Id.HasValue && dto.Id > 0)
        {
            // Edit Mode
            account = await _context.Accounts
                .Include(a => a.AccountCategory)
                .FirstOrDefaultAsync(a => a.Id == dto.Id);

            if (account == null || account.UserId != userId)
                return Result.Failure<AccountDto>(new Error("Account.NotFound", "Account not found."));

            _mapper.Map(dto, account);
            _context.Accounts.Update(account);
        }
        else
        {
            // Create Mode
            account = _mapper.Map<Account>(dto);
            account.UserId = userId;
            _context.Accounts.Add(account);
        }

        await _context.SaveChangesAsync();

        // Reload to get populated fields (like Category name if needed, or DB generated values)
        // Note: For simple update, memory object is usually enough, but if we need relations:
        // account = await _context.Accounts.Include(a => a.AccountCategory).FirstAsync(a => a.Id == account.Id); 

        var resultDto = _mapper.Map<AccountDto>(account);
        return Result.Success(resultDto);
    }

    public async Task<Result<bool>> DeleteAccountAsync(string userId, int accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null || account.UserId != userId)
            return Result.Failure<bool>(new Error("Account.NotFound", "Account not found."));

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return Result.Success(true);
    }
}