using Application.Contracts;
using Application.DTOs.Transactions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.BackgroundWorkers;

public class RecurringTransactionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringTransactionWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public RecurringTransactionWorker(IServiceProvider serviceProvider, ILogger<RecurringTransactionWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recurring Transaction Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRecurringTransactionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing recurring transactions.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Recurring Transaction Worker is stopping.");
    }

    private async Task ProcessRecurringTransactionsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var now = DateTime.UtcNow;
        
        // Fetch all active recurring transactions that are due
        var dueTransactions = await context.RecurringTransactions
            .Where(rt => rt.IsActive && !rt.IsDeleted && rt.NextProcessDate <= now)
            .ToListAsync(ct);

        if (dueTransactions.Count == 0) return;

        _logger.LogInformation("Processing {Count} due recurring transactions.", dueTransactions.Count);

        foreach (var rt in dueTransactions)
        {
            try
            {
                // Create the actual transaction
                var upsertDto = new UpsertTransactionDto
                {
                    Description = rt.Description,
                    Amount = rt.Amount,
                    Type = rt.Type,
                    Date = rt.NextProcessDate,
                    TransactionCategoryId = rt.TransactionCategoryId
                };

                var result = await transactionService.UpsertTransactionAsync(rt.UserId, rt.AccountId, upsertDto);
                
                if (result.IsSuccess)
                {
                    // Update the recurring transaction state
                    rt.LastProcessedDate = rt.NextProcessDate;
                    rt.NextProcessDate = CalculateNextDate(rt.NextProcessDate, rt.Frequency);
                    
                    // If an EndDate exists and we've passed it, deactivate
                    if (rt.EndDate.HasValue && rt.NextProcessDate > rt.EndDate.Value)
                    {
                        rt.IsActive = false;
                    }

                    context.RecurringTransactions.Update(rt);
                }
                else
                {
                    _logger.LogWarning("Failed to process recurring transaction {Id}: {Error}", rt.Id, result.Error.Description);
                    // Optionally: Increment a retry count or notify someone
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process recurring transaction {Id}", rt.Id);
            }
        }

        await context.SaveChangesAsync(ct);
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
