using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public enum WorkflowStatus { Pending, Approved, Rejected }

/// <summary>
/// A running approval process for one specific entity record.
/// Created when a user submits a Policy/Risk/ControlTest for approval.
/// </summary>
public class WorkflowInstance : AuditableEntity
{
    public int WorkflowDefinitionId { get; set; }
    public WorkflowDefinition? Definition { get; set; }

    /// <summary>Policy | Risk | ControlTest</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>PK of the entity being approved</summary>
    public int EntityId { get; set; }

    public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;

    /// <summary>Stage currently awaiting action (null = process complete)</summary>
    public int? CurrentStageOrder { get; set; }

    /// <summary>User who submitted this entity for approval</summary>
    public int SubmittedById { get; set; }
    public UserProfile? SubmittedBy { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────

    public ICollection<WorkflowInstanceAction> Actions { get; set; } = [];
}
