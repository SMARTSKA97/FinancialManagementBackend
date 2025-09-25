namespace FinancialPlanner.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string UserId { get; set; } // To scope categories to each user
}