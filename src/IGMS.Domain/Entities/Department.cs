using IGMS.Domain.Common;
using IGMS.Domain.Interfaces;

namespace IGMS.Domain.Entities;

/// <summary>
/// Organizational unit. Supports unlimited hierarchy levels via self-referencing ParentId.
/// Level labels (وزارة/إدارة/قسم/وحدة) come from tenant config – not hardcoded here.
/// </summary>
public class Department : AuditableEntity, ILocalizable
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    /// <summary>Short code used in reports (e.g. "IT", "HR")</summary>
    public string Code { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }

    /// <summary>Hierarchy level: 1=Top (Ministry), 2, 3, 4...</summary>
    public int Level { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    // ── Relationships ─────────────────────────────────────────────────────────

    /// <summary>null = root department (top of hierarchy)</summary>
    public int? ParentId { get; set; }
    public Department? Parent { get; set; }
    public ICollection<Department> Children { get; set; } = [];

    /// <summary>The employee who manages this department</summary>
    public int? ManagerId { get; set; }
    public UserProfile? Manager { get; set; }

    public ICollection<UserProfile> Members { get; set; } = [];
}
