using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// Named GovernanceTask to avoid conflict with System.Threading.Tasks.Task.
/// Mapped to table [Tasks] in DB.
/// </summary>
public class GovernanceTask : AuditableEntity
{
    public string  TitleAr       { get; set; } = string.Empty;
    public string  TitleEn       { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }

    public TaskStatus   Status   { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    public int? AssignedToId { get; set; }
    public UserProfile? AssignedTo { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    /// <summary>Optional: links this task to a mitigation risk.</summary>
    public int? RiskId { get; set; }
    public Risk? Risk { get; set; }
}

public enum TaskStatus
{
    Todo        = 0,
    InProgress  = 1,
    Done        = 2,
    Cancelled   = 3,
}

public enum TaskPriority
{
    Low      = 0,
    Medium   = 1,
    High     = 2,
    Critical = 3,
}
