namespace IGMS.Domain.Entities;

/// <summary>
/// Junction table: User ↔ Role (many-to-many).
/// Supports expiry date for temporary role assignments.
/// </summary>
public class UserRole
{
    public int UserId { get; set; }
    public UserProfile User { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string AssignedBy { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>null = permanent assignment</summary>
    public DateTime? ExpiresAt { get; set; }

    public bool IsActive => ExpiresAt == null || ExpiresAt > DateTime.UtcNow;
}
