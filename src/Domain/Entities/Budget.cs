using Domain.Enums;

namespace Domain.Entities;

public class Budget : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int? TransactionCategoryId { get; set; }
    public TransactionCategory? TransactionCategory { get; set; }

    public decimal Amount { get; set; }
    
    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;
    
    public DateTime StartDate { get; set; }
    // Null EndDate means it's a recurring/ongoing budget constraint
    public DateTime? EndDate { get; set; }
}
