namespace FinancialPlanner.Domain.Entities;

public abstract class AuditLog
{
    public int Id { get; set; }
    public int EntityId { get; set; }
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public string Action { get; set; } = string.Empty; // Removed 'required', provided default
    public string ChangedByUserId { get; set; } = string.Empty; // Removed 'required', provided default
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}