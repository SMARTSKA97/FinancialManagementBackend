namespace FinancialPlanner.Domain.Entities;

public class Account
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Balance { get; set; }
    public string? Category { get; set; }
    public required string UserId { get; set; }

    // Add navigation property for related transactions
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}