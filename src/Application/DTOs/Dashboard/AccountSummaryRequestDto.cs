namespace Application.DTOs.Dashboard;

public class AccountSummaryRequestDto
{
    public int AccountId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
