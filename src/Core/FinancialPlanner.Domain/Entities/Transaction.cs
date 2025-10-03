using FinancialPlanner.Domain.Enums;

namespace FinancialPlanner.Domain.Entities;

public class Transaction : BaseEntity
{
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public int? TransactionCategoryId { get; set; }
    public TransactionCategory? TransactionCategory { get; set; }
}