using System.ComponentModel.DataAnnotations;
using IGMS.Domain.Entities;

namespace IGMS.Application.Common.Models;

public class ComplianceMappingDto
{
    public int                Id        { get; set; }
    public string             EntityType { get; set; } = string.Empty;
    public int                EntityId  { get; set; }
    public ComplianceFramework Framework { get; set; }
    public string             FrameworkLabel { get; set; } = string.Empty;
    public string?            Clause    { get; set; }
    public string?            Notes     { get; set; }
    public DateTime           CreatedAt { get; set; }
    public string             CreatedBy { get; set; } = string.Empty;
}

public class AddComplianceMappingRequest
{
    [Required] public string             EntityType { get; set; } = string.Empty;
    [Required] public int                EntityId   { get; set; }
    [Required] public ComplianceFramework Framework  { get; set; }
    [MaxLength(100)] public string?      Clause     { get; set; }
    [MaxLength(500)] public string?      Notes      { get; set; }
}

// ── Compliance Report DTOs ─────────────────────────────────────────────────────

public class ComplianceReportData
{
    public DateTime ReportDate    { get; set; }
    public string   OrgName       { get; set; } = string.Empty;
    public int      PolTotal      { get; set; }
    public int      RskTotal      { get; set; }
    public double   AvgCompliance { get; set; }
    public List<FrameworkCoverage> Frameworks { get; set; } = new();
}

public class FrameworkCoverage
{
    public ComplianceFramework Framework  { get; set; }
    public string  Label      { get; set; } = string.Empty;
    public string  Group      { get; set; } = string.Empty;
    public int     PolCovered { get; set; }
    public int     PolTotal   { get; set; }
    public double  PolPct     { get; set; }
    public int     RskCovered { get; set; }
    public int     RskTotal   { get; set; }
    public double  RskPct     { get; set; }
    public double  OverallPct { get; set; }
}
