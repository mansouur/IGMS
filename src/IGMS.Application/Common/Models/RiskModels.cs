using System.ComponentModel.DataAnnotations;
using IGMS.Domain.Entities;

namespace IGMS.Application.Common.Models;

public class RiskListDto
{
    public int          Id               { get; set; }
    public string       TitleAr          { get; set; } = string.Empty;
    public string       TitleEn          { get; set; } = string.Empty;
    public string       Code             { get; set; } = string.Empty;
    public RiskCategory Category         { get; set; }
    public RiskStatus   Status           { get; set; }
    public int          Likelihood       { get; set; }
    public int          Impact           { get; set; }
    public int          RiskScore        { get; set; }
    public string?      DepartmentNameAr { get; set; }
    public string?      OwnerNameAr      { get; set; }
    public DateTime     CreatedAt        { get; set; }
}

public class RiskDetailDto : RiskListDto
{
    public string? DescriptionAr    { get; set; }
    public string? MitigationPlanAr { get; set; }
    public int?    DepartmentId     { get; set; }
    public int?    OwnerId          { get; set; }
    public int     LinkedTasksCount { get; set; }
}

public class SaveRiskRequest
{
    public int Id { get; set; }
    [Required] public string TitleAr { get; set; } = string.Empty;
    public string TitleEn            { get; set; } = string.Empty;
    [Required] public string Code    { get; set; } = string.Empty;
    public string?      DescriptionAr    { get; set; }
    public string?      MitigationPlanAr { get; set; }
    public RiskCategory Category         { get; set; }
    public RiskStatus   Status           { get; set; }
    [Range(1,5)] public int Likelihood   { get; set; } = 1;
    [Range(1,5)] public int Impact       { get; set; } = 1;
    public int?         DepartmentId     { get; set; }
    public int?         OwnerId          { get; set; }
}

/// <summary>Lightweight projection used by the 5×5 heat map.</summary>
public class RiskHeatMapItemDto
{
    public int        Id         { get; set; }
    public string     TitleAr    { get; set; } = string.Empty;
    public string     Code       { get; set; } = string.Empty;
    public int        Likelihood { get; set; }
    public int        Impact     { get; set; }
    public int        RiskScore  { get; set; }
    public RiskStatus Status     { get; set; }
    public string?    OwnerNameAr { get; set; }
}

public class RiskQuery
{
    public int           Page     { get; set; } = 1;
    public int           PageSize { get; set; } = 20;
    public string?       Search   { get; set; }
    public RiskStatus?   Status   { get; set; }
    public RiskCategory? Category { get; set; }
}

public class RiskKpiLinkDto
{
    public int     MappingId  { get; set; }
    public int     KpiId      { get; set; }
    public string  KpiCode    { get; set; } = string.Empty;
    public string  KpiTitleAr { get; set; } = string.Empty;
    public string? Notes      { get; set; }
}

public class AddRiskKpiLinkRequest
{
    public int     KpiId { get; set; }
    public string? Notes { get; set; }
}
