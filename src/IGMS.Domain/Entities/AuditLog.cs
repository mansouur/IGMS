namespace IGMS.Domain.Entities;

/// <summary>
/// Immutable audit trail. Every Create/Update/Delete on any entity is logged here.
/// Never soft-deleted – audit logs are permanent by design.
/// OldValues/NewValues stored as JSON for full diff visibility.
/// </summary>
public class AuditLog
{
    public long Id { get; set; } // long – high volume table

    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Created | Updated | Deleted | Login | Logout | PermissionChanged</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>JSON snapshot of values before change (null for Created)</summary>
    public string? OldValues { get; set; }

    /// <summary>JSON snapshot of values after change (null for Deleted)</summary>
    public string? NewValues { get; set; }

    public int? UserId { get; set; }
    public UserProfile? User { get; set; }
    public string Username { get; set; } = string.Empty;

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
