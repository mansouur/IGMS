using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// A reusable approval-workflow template.
/// One definition applies to one entity type (Policy | Risk | ControlTest).
/// Each tenant can have multiple definitions per entity type (only one active at a time).
/// </summary>
public class WorkflowDefinition : AuditableEntity
{
    /// <summary>Policy | Risk | ControlTest</summary>
    public string EntityType { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }

    /// <summary>Only one definition per EntityType should be active at a time</summary>
    public bool IsActive { get; set; } = true;

    // ── Navigation ───────────────────────────────────────────────────────────

    public ICollection<WorkflowStage>    Stages    { get; set; } = [];
    public ICollection<WorkflowInstance> Instances { get; set; } = [];
}
