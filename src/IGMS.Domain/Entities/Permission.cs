using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// Fine-grained permission scoped to a module and action.
/// Code format: "{MODULE}.{ACTION}" (e.g. "RACI.APPROVE", "POLICY.PUBLISH")
/// New modules are added here without changing role logic.
/// </summary>
public class Permission : AuditableEntity
{
    /// <summary>System module: USERS | DEPARTMENTS | RACI | POLICIES | TASKS | KPI | RISKS | REPORTS | SETTINGS</summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>Action: READ | CREATE | UPDATE | DELETE | APPROVE | PUBLISH | EXPORT | ASSIGN | MANAGE</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Composite unique key: "RACI.APPROVE"</summary>
    public string Code { get; set; } = string.Empty;

    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;

    // ── Relationships ─────────────────────────────────────────────────────────

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
