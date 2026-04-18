using IGMS.Domain.Common;
using IGMS.Domain.Interfaces;

namespace IGMS.Domain.Entities;

/// <summary>
/// System role (RBAC). Groups permissions together.
/// System roles (IsSystemRole=true) cannot be deleted or renamed.
/// </summary>
public class Role : AuditableEntity, ILocalizable
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    /// <summary>Unique machine-readable code: ADMIN | MANAGER | USER | VIEWER</summary>
    public string Code { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }

    /// <summary>System roles are seeded and protected from deletion</summary>
    public bool IsSystemRole { get; set; } = false;

    public bool IsActive { get; set; } = true;

    // ── Relationships ─────────────────────────────────────────────────────────

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
