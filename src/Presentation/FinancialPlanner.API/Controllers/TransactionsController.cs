using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Transactions; // Add this
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enums;
using FinancialPlanner.Infrastructure.Persistence; // Add this
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinancialPlanner.API.Controllers;

[ApiController]
[Route("api/accounts/{accountId}/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly ApplicationDbContext _context; // Inject DbContext for transaction logic

    public TransactionsController(
        IGenericRepository<Account> accountRepository,
        ApplicationDbContext context) // Update constructor
    {
        _accountRepository = accountRepository;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactionsForAccount(int accountId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = await _accountRepository.GetByIdAsync(accountId);

        if (account == null || account.UserId != userId)
        {
            return NotFound("Account not found.");
        }

        // Use the DbContext to efficiently query related data
        var transactions = await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        return Ok(transactions);
    }

    // --- ADD THIS NEW METHOD ---
    [HttpPost]
    public async Task<IActionResult> CreateTransaction(int accountId, [FromBody] UpsertTransactionDto dto)
    {
        // 1. Security Check: Verify user owns the account
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = await _accountRepository.GetByIdAsync(accountId);

        if (account == null || account.UserId != userId)
        {
            return NotFound("Account not found.");
        }

        // 2. Create the new transaction entity
        var transaction = new Transaction
        {
            Description = dto.Description,
            Amount = dto.Amount,
            Date = dto.Date,
            Type = dto.Type,
            AccountId = accountId,
            CategoryId = dto.CategoryId,
            Account = account
        };

        // 3. Business Logic: Update the account's balance
        if (transaction.Type == Domain.Enums.TransactionType.Income)
        {
            account.Balance += transaction.Amount;
        }
        else // Expense
        {
            account.Balance -= transaction.Amount;
        }

        // 4. Save both changes to the database
        await _context.Transactions.AddAsync(transaction);
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransactionsForAccount), new { accountId = account.Id, id = transaction.Id }, transaction);
    }

    // --- ADD THIS UPDATE METHOD ---
    [HttpPut("{transactionId}")]
    public async Task<IActionResult> UpdateTransaction(int accountId, int transactionId, [FromBody] UpsertTransactionDto dto)
    {
        // 1. Security Check: Verify user owns the account
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = await _context.Accounts.FindAsync(accountId);

        if (account == null || account.UserId != userId)
        {
            return NotFound("Account not found.");
        }

        // 2. Find the specific transaction
        var transaction = await _context.Transactions.FindAsync(transactionId);

        if (transaction == null || transaction.AccountId != accountId)
        {
            return NotFound("Transaction not found.");
        }

        // 3. Business Logic: Revert the old transaction amount from the account balance
        if (transaction.Type == TransactionType.Income)
        {
            account.Balance -= transaction.Amount;
        }
        else // Expense
        {
            account.Balance += transaction.Amount;
        }

        // 4. Update transaction with new values
        transaction.Description = dto.Description;
        transaction.Amount = dto.Amount;
        transaction.Date = dto.Date;
        transaction.Type = dto.Type;
        transaction.CategoryId = dto.CategoryId;

        // 5. Business Logic: Apply the new transaction amount to the account balance
        if (transaction.Type == TransactionType.Income)
        {
            account.Balance += transaction.Amount;
        }
        else // Expense
        {
            account.Balance -= transaction.Amount;
        }

        // 6. Save changes
        _context.Transactions.Update(transaction);
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // --- ADD THIS DELETE METHOD ---
    [HttpDelete("{transactionId}")]
    public async Task<IActionResult> DeleteTransaction(int accountId, int transactionId)
    {
        // 1. Security Check: Verify user owns the account
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = await _context.Accounts.FindAsync(accountId);

        if (account == null || account.UserId != userId)
        {
            return NotFound("Account not found.");
        }

        // 2. Find the specific transaction
        var transaction = await _context.Transactions.FindAsync(transactionId);

        if (transaction == null || transaction.AccountId != accountId)
        {
            return NotFound("Transaction not found.");
        }

        // 3. Business Logic: Revert the transaction amount from the account balance
        if (transaction.Type == TransactionType.Income)
        {
            account.Balance -= transaction.Amount;
        }
        else // Expense
        {
            account.Balance += transaction.Amount;
        }

        // 4. Delete the transaction and update the account
        _context.Transactions.Remove(transaction);
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}