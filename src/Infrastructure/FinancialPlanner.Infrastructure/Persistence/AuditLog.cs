using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FinancialPlanner.Infrastructure.Persistence;

public class AuditEntry
{
    public AuditEntry(EntityEntry entry) { Entry = entry; }
    public EntityEntry Entry { get; }
    public string UserId { get; set; } = string.Empty; // Removed 'required'
    public string TableName { get; set; } = string.Empty; // Removed 'required'
    public string Action { get; set; } = string.Empty; // Removed 'required'
    public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
}