namespace FinancialPlanner.Application.DTOs.Dashboard;

public class AccountSummaryDto
{
    public decimal CurrentBalance { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
}