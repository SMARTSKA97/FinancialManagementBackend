namespace FinancialPlanner.Domain.Entities;

public class Account : BaseEntity
{
    public required string Name { get; set; }
    public decimal Balance { get; set; }
    public required string UserId { get; set; }

    public int AccountCategoryId { get; set; }
    public AccountCategory AccountCategory { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}