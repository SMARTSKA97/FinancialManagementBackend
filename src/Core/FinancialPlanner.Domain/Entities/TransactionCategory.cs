namespace FinancialPlanner.Domain.Entities;

public class TransactionCategory : BaseEntity
{
    public required string Name { get; set; }
    public required string UserId { get; set; }
    public bool IsTransferCategory { get; set; } = false;
}