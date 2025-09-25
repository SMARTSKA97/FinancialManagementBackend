using FinancialPlanner.Domain.Enums;

namespace FinancialPlanner.Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }

    public int AccountId { get; set; }
    public Category? Category { get; set; }
    public int? CategoryId { get; set; }

    // Remove the 'required' keyword from this navigation property
    public Account Account { get; set; }
}