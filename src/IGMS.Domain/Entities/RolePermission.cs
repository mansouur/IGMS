namespace IGMS.Domain.Entities;

/// <summary>
/// Junction table: Role ↔ Permission (many-to-many).
/// Defines what each role can do across all modules.
/// </summary>
public class RolePermission
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string GrantedBy { get; set; } = string.Empty;
}
