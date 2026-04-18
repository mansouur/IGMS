using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public enum IncidentSeverity { Low = 1, Medium = 2, High = 3, Critical = 4 }
public enum IncidentStatus   { Open, UnderReview, Resolved, Closed }

/// <summary>
/// A security/governance incident that may be linked to a Risk and/or a Task.
/// </summary>
public class Incident : AuditableEntity
{
    public string  TitleAr       { get; set; } = string.Empty;
    public string? TitleEn       { get; set; }
    public string? DescriptionAr { get; set; }

    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;
    public IncidentStatus   Status   { get; set; } = IncidentStatus.Open;

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? ReportedById { get; set; }
    public UserProfile? ReportedBy { get; set; }

    /// <summary>Linked risk (optional).</summary>
    public int? RiskId { get; set; }
    public Risk? Risk  { get; set; }

    /// <summary>Remediation task (optional).</summary>
    public int? TaskId { get; set; }
    public GovernanceTask? Task { get; set; }

    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt    { get; set; }
}
