namespace IGMS.Application.Common.Models;

// ── List DTO ──────────────────────────────────────────────────────────────────

public class IncidentListDto
{
    public int     Id             { get; set; }
    public string  TitleAr        { get; set; } = string.Empty;
    public string? TitleEn        { get; set; }
    public string  Severity       { get; set; } = string.Empty;
    public string  Status         { get; set; } = string.Empty;
    public DateTime OccurredAt    { get; set; }
    public string?  DepartmentName { get; set; }
    public string?  ReportedByName { get; set; }
    public int?     RiskId         { get; set; }
    public string?  RiskTitleAr    { get; set; }
}

// ── Detail DTO ────────────────────────────────────────────────────────────────

public class IncidentDetailDto : IncidentListDto
{
    public string?   DescriptionAr   { get; set; }
    public int?      DepartmentId    { get; set; }
    public int?      ReportedById    { get; set; }
    public int?      TaskId          { get; set; }
    public string?   TaskTitleAr     { get; set; }
    public string?   ResolutionNotes { get; set; }
    public DateTime? ResolvedAt      { get; set; }
    public DateTime  CreatedAt       { get; set; }
}

// ── Save request ──────────────────────────────────────────────────────────────

public class SaveIncidentRequest
{
    public string  TitleAr        { get; set; } = string.Empty;
    public string? TitleEn        { get; set; }
    public string? DescriptionAr  { get; set; }
    public string  Severity       { get; set; } = "Medium";
    public string  Status         { get; set; } = "Open";
    public DateTime OccurredAt    { get; set; } = DateTime.UtcNow;
    public int?    DepartmentId   { get; set; }
    public int?    RiskId         { get; set; }
    public int?    TaskId         { get; set; }
    public string? ResolutionNotes { get; set; }
}
