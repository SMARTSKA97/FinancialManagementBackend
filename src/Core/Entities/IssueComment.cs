namespace Domain.Entities;

public class IssueComment : BaseEntity
{
    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null!;
    
    public required string Content { get; set; }
    public string? CreatorUserId { get; set; } // Should link to User logically
    
    // Structured Data (JSON serialized or columns)
    public string? ExpectedBehavior { get; set; }
    public string? ActualBehavior { get; set; }
    public bool HasWorkaround { get; set; }
    public string? StructuredMetadata { get; set; }
    
    // Community Signals
    public bool IsHelpful { get; set; }
    public bool IsRootCause { get; set; }
    public bool IsReproConfirmed { get; set; }
}
