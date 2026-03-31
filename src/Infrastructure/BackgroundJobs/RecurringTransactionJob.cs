using Application.Contracts;
using Application.DTOs.Transactions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

public class RecurringTransactionJob
{
    private readonly IApplicationDbContext _context;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<RecurringTransactionJob> _logger;

    public RecurringTransactionJob(
        IApplicationDbContext context, 
        ITransactionService transactionService, 
        ILogger<RecurringTransactionJob> logger)
    {
        _context = context;
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task ProcessRecurringTransactionsAsync()
    {
        _logger.LogInformation("Hangfire Job: Processing Recurring Transactions...");

        var now = DateTime.UtcNow;
        
        // Fetch all active recurring transactions that are due
        var dueTransactions = await _context.RecurringTransactions
            .Where(rt => rt.IsActive && !rt.IsDeleted && rt.NextProcessDate <= now)
            .ToListAsync();

        if (dueTransactions.Count == 0)
        {
            _logger.LogInformation("No recurring transactions are due.");
            return;
        }

        _logger.LogInformation("Processing {Count} due recurring transactions.", dueTransactions.Count);

        foreach (var rt in dueTransactions)
        {
            try
            {
                var upsertDto = new UpsertTransactionDto
                {
                    Description = rt.Description,
                    Amount = rt.Amount,
                    Type = rt.Type,
                    Date = rt.NextProcessDate,
                    TransactionCategoryId = rt.TransactionCategoryId
                };

                var result = await _transactionService.UpsertTransactionAsync(rt.UserId, rt.AccountId, upsertDto);
                
                if (result.IsSuccess)
                {
                    rt.LastProcessedDate = rt.NextProcessDate;
                    rt.NextProcessDate = CalculateNextDate(rt.NextProcessDate, rt.Frequency);
                    
                    if (rt.EndDate.HasValue && rt.NextProcessDate > rt.EndDate.Value)
                    {
                        rt.IsActive = false;
                    }

                    _context.RecurringTransactions.Update(rt);
                }
                else
                {
                    // Hangfire can automatically retry if we throw an exception,
                    // but for partial success we might just log it and rely on the next run.
                    // For a financial grade system, if one fails we throw to use Hangfire's retry policy!
                    throw new Exception($"Failed to process recurring transaction {rt.Id}: {result.Error.Description}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process recurring transaction {Id}", rt.Id);
                // Re-throw so Hangfire registers this job instance as failed and applies exponential backoff
                throw;
            }
        }

        await _context.SaveChangesAsync(default);
        _logger.LogInformation("Finished processing recurring transactions.");
    }

    private static DateTime CalculateNextDate(DateTime current, RecurrenceFrequency frequency)
    {
        return frequency switch
        {
            RecurrenceFrequency.Daily => current.AddDays(1),
            RecurrenceFrequency.Weekly => current.AddDays(7),
            RecurrenceFrequency.Monthly => current.AddMonths(1),
            RecurrenceFrequency.Yearly => current.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, null)
        };
    }
}
