using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FinancialPlanner.Infrastructure.Persistence;

// This helper class is used by the DbContext for logging
public class AuditEntry
{
    public AuditEntry(EntityEntry entry) { Entry = entry; }
    public EntityEntry Entry { get; }
    public string UserId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
}