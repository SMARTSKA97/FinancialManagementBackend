namespace Domain.Entities;

public class Subscription : BaseEntity
{
    public required string UserId { get; set; }
    
    public required string Name { get; set; }
    public string? Tag { get; set; }
    public string? CancellationUrl { get; set; }
    
    // 1:1 Relationship with the Execution Layer
    public int RecurringTransactionId { get; set; }
    public RecurringTransaction RecurringTransaction { get; set; } = null!;
}
