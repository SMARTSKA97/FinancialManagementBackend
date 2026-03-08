using Domain.Enums;

namespace Application.DTOs.Budgets;

public class BudgetProgressDto
{
    public int BudgetId { get; set; }
    public int? TransactionCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount => BudgetAmount - SpentAmount;
    public decimal PercentageUsed => BudgetAmount > 0 ? (SpentAmount / BudgetAmount) * 100 : 0;
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
