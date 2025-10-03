using FinancialPlanner.Domain.Enums;

namespace FinancialPlanner.Application.DTOs.Transactions;

public class TransactionDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public int AccountId { get; set; }
    public string? CategoryName { get; set; }
}