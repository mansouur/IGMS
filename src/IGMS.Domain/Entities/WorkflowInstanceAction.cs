using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public enum WorkflowDecision { Approved, Rejected, Commented }

/// <summary>
/// Immutable history record: one row per decision/comment on a WorkflowInstance stage.
/// </summary>
public class WorkflowInstanceAction : AuditableEntity
{
    public int WorkflowInstanceId { get; set; }
    public WorkflowInstance? Instance { get; set; }

    /// <summary>Which stage order this action was taken on</summary>
    public int StageOrder { get; set; }

    public int ActorId { get; set; }
    public UserProfile? Actor { get; set; }

    public WorkflowDecision Decision { get; set; }

    public string? Comment { get; set; }

    public DateTime ActedAt { get; set; } = DateTime.UtcNow;
}
