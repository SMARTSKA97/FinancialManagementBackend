using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Accounts;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinancialPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // <-- This attribute protects all endpoints in this controller
public class AccountsController : ControllerBase
{
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IMapper _mapper;
    public AccountsController(IGenericRepository<Account> accountRepository, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var allAccounts = await _accountRepository.GetAllAsync();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userAccounts = allAccounts.Where(a => a.UserId == userId);
        var accountDtos = _mapper.Map<IReadOnlyList<AccountDto>>(userAccounts);
        return Ok(accountDtos);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        var account = new Account
        {
            Name = createAccountDto.Name,
            Balance = createAccountDto.Balance,
            UserId = userId
        };

        var newAccount = await _accountRepository.AddAsync(account);

        return CreatedAtAction(nameof(GetAccounts), new { id = newAccount.Id }, newAccount);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountDto updateAccountDto)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (account == null || account.UserId != userId)
        {
            return NotFound(); // Don't reveal if the account exists but belongs to someone else
        }

        // Update properties
        account.Name = updateAccountDto.Name;
        account.Balance = updateAccountDto.Balance;

        _accountRepository.Update(account);

        return NoContent(); // Standard response for a successful update
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (account == null || account.UserId != userId)
        {
            return NotFound();
        }

        _accountRepository.Delete(account);

        return NoContent(); // Standard response for a successful delete
    }
}