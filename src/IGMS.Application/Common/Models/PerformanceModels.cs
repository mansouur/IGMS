namespace IGMS.Application.Common.Models;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record PerformanceGoalDto(
    int     Id,
    string  TitleAr,
    string? DescriptionAr,
    decimal Weight,
    decimal? TargetValue,
    decimal? ActualValue,
    decimal? Rating,
    string  Status
);

public record PerformanceReviewListDto(
    int      Id,
    int      EmployeeId,
    string   EmployeeName,
    int      ReviewerId,
    string   ReviewerName,
    string   Period,
    int      Year,
    string   Status,
    decimal? OverallRating,
    string?  DepartmentName,
    int      GoalCount,
    DateTime CreatedAt
);

public record PerformanceReviewDetailDto(
    int      Id,
    int      EmployeeId,
    string   EmployeeName,
    int      ReviewerId,
    string   ReviewerName,
    string   Period,
    int      Year,
    string   Status,
    decimal? OverallRating,
    string?  StrengthsAr,
    string?  AreasForImprovementAr,
    string?  CommentsAr,
    string?  EmployeeCommentsAr,
    string?  RejectReason,
    DateTime? SubmittedAt,
    DateTime? ApprovedAt,
    string?  DepartmentName,
    DateTime CreatedAt,
    List<PerformanceGoalDto> Goals
);

// ── Requests ──────────────────────────────────────────────────────────────────

public class SaveGoalRequest
{
    public int?    Id            { get; set; }
    public string  TitleAr       { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public decimal Weight        { get; set; }
    public decimal? TargetValue  { get; set; }
    public decimal? ActualValue  { get; set; }
    public decimal? Rating       { get; set; }
    public string  Status        { get; set; } = "Pending";
}

public class SavePerformanceReviewRequest
{
    public int     EmployeeId           { get; set; }
    public int     ReviewerId           { get; set; }
    public string  Period               { get; set; } = "Annual";
    public int     Year                 { get; set; } = DateTime.UtcNow.Year;
    public int?    DepartmentId         { get; set; }
    public decimal? OverallRating       { get; set; }
    public string? StrengthsAr          { get; set; }
    public string? AreasForImprovementAr{ get; set; }
    public string? CommentsAr           { get; set; }
    public string? EmployeeCommentsAr   { get; set; }
    public List<SaveGoalRequest> Goals  { get; set; } = [];
}

public class RejectReviewRequest
{
    public string? Reason { get; set; }
}

// ── Query ─────────────────────────────────────────────────────────────────────

public class PerformanceQuery
{
    public int     Page         { get; set; } = 1;
    public int     PageSize     { get; set; } = 20;
    public string? Search       { get; set; }
    public string? Status       { get; set; }
    public string? Period       { get; set; }
    public int?    Year         { get; set; }
    public int?    EmployeeId   { get; set; }
    public int?    DepartmentId { get; set; }
}
