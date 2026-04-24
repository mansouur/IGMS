using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public enum ReviewStatus { Draft, Submitted, Approved, Rejected }
public enum ReviewPeriod { Q1, Q2, Q3, Q4, Annual, Probation }
public enum GoalStatus   { Pending, Achieved, PartiallyAchieved, NotAchieved }

public class PerformanceReview : AuditableEntity
{
    public int EmployeeId { get; set; }
    public UserProfile Employee { get; set; } = null!;

    public int ReviewerId { get; set; }
    public UserProfile Reviewer { get; set; } = null!;

    public ReviewPeriod Period { get; set; } = ReviewPeriod.Annual;
    public int Year { get; set; } = DateTime.UtcNow.Year;

    public ReviewStatus Status { get; set; } = ReviewStatus.Draft;

    /// <summary>Weighted overall score 1–5.</summary>
    public decimal? OverallRating { get; set; }

    public string? StrengthsAr            { get; set; }
    public string? AreasForImprovementAr  { get; set; }
    public string? CommentsAr             { get; set; }   // reviewer comments
    public string? EmployeeCommentsAr     { get; set; }   // self-assessment

    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt  { get; set; }
    public string?   RejectReason { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public List<PerformanceGoal> Goals { get; set; } = [];
}

public class PerformanceGoal : AuditableEntity
{
    public int ReviewId { get; set; }
    public PerformanceReview Review { get; set; } = null!;

    public string  TitleAr       { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }

    /// <summary>Relative weight percentage (0-100); sum across goals should equal 100.</summary>
    public decimal Weight { get; set; }

    public decimal? TargetValue { get; set; }
    public decimal? ActualValue { get; set; }
    public decimal? Rating      { get; set; }  // 1-5

    public GoalStatus Status { get; set; } = GoalStatus.Pending;
}
