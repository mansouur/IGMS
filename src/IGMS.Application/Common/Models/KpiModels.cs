using System.ComponentModel.DataAnnotations;
using IGMS.Domain.Entities;

namespace IGMS.Application.Common.Models;

public class KpiListDto
{
    public int       Id               { get; set; }
    public string    TitleAr          { get; set; } = string.Empty;
    public string    TitleEn          { get; set; } = string.Empty;
    public string    Code             { get; set; } = string.Empty;
    public string?   Unit             { get; set; }
    public decimal   TargetValue      { get; set; }
    public decimal   ActualValue      { get; set; }
    public int       Year             { get; set; }
    public int?      Quarter          { get; set; }
    public KpiStatus Status           { get; set; }
    public string?   DepartmentNameAr { get; set; }
    public string?   OwnerNameAr      { get; set; }
    public DateTime  CreatedAt        { get; set; }
}

public class KpiDetailDto : KpiListDto
{
    public int? DepartmentId { get; set; }
    public int? OwnerId      { get; set; }
}

public class SaveKpiRequest
{
    public int Id { get; set; }
    [Required] public string TitleAr { get; set; } = string.Empty;
    public string TitleEn            { get; set; } = string.Empty;
    [Required] public string Code    { get; set; } = string.Empty;
    public string?   Unit         { get; set; }
    public decimal   TargetValue  { get; set; }
    public decimal   ActualValue  { get; set; }
    public int       Year         { get; set; } = DateTime.UtcNow.Year;
    [Range(1,4)] public int? Quarter { get; set; }
    public KpiStatus Status       { get; set; }
    public int?      DepartmentId { get; set; }
    public int?      OwnerId      { get; set; }
}

public class KpiQuery
{
    public int        Page     { get; set; } = 1;
    public int        PageSize { get; set; } = 20;
    public string?    Search   { get; set; }
    public KpiStatus? Status   { get; set; }
    public int?       Year     { get; set; }
}

public class KpiRiskLinkDto
{
    public int        MappingId   { get; set; }
    public int        RiskId      { get; set; }
    public string     RiskCode    { get; set; } = string.Empty;
    public string     RiskTitleAr { get; set; } = string.Empty;
    public int        RiskScore   { get; set; }
    public RiskStatus RiskStatus  { get; set; }
    public string?    Notes       { get; set; }
}

public class KpiScorecardItemDto
{
    public int       Id             { get; set; }
    public string    TitleAr        { get; set; } = string.Empty;
    public string    Code           { get; set; } = string.Empty;
    public double    AchievementPct { get; set; }
    public KpiStatus Status         { get; set; }
}

public class DepartmentScorecardDto
{
    public int?    DepartmentId      { get; set; }
    public string  DepartmentNameAr  { get; set; } = "غير محدد";
    public int     KpiCount          { get; set; }
    public int     OnTrackCount      { get; set; }
    public int     AtRiskCount       { get; set; }
    public int     BehindCount       { get; set; }
    public double  AvgAchievementPct { get; set; }
    public int     Score             { get; set; }
    public List<KpiScorecardItemDto> Kpis { get; set; } = new();
}
