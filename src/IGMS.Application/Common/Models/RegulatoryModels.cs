using System.ComponentModel.DataAnnotations;
using IGMS.Domain.Entities;

namespace IGMS.Application.Common.Models;

// ── Framework DTOs ────────────────────────────────────────────────────────────

public class RegulatoryFrameworkDto
{
    public int    Id           { get; set; }
    public string Code         { get; set; } = string.Empty;
    public string NameAr       { get; set; } = string.Empty;
    public string NameEn       { get; set; } = string.Empty;
    public string Version      { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public bool   IsActive     { get; set; }
    public int    ControlCount { get; set; }
    public int    MappedCount  { get; set; }   // how many controls have ≥1 mapping by current tenant
}

// ── Control DTOs ──────────────────────────────────────────────────────────────

public class RegulatoryControlDto
{
    public int    Id          { get; set; }
    public string ControlCode { get; set; } = string.Empty;
    public string DomainAr    { get; set; } = string.Empty;
    public string DomainEn    { get; set; } = string.Empty;
    public string TitleAr     { get; set; } = string.Empty;
    public string TitleEn     { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }

    /// <summary>Mappings made by the current tenant for this control</summary>
    public List<ControlMappingDto> Mappings { get; set; } = [];
}

// ── Mapping DTOs ──────────────────────────────────────────────────────────────

public class ControlMappingDto
{
    public int    Id                 { get; set; }
    public int    RegulatoryControlId { get; set; }
    public string ControlCode        { get; set; } = string.Empty;
    public string ControlTitleAr     { get; set; } = string.Empty;
    public string EntityType         { get; set; } = string.Empty;
    public int    EntityId           { get; set; }
    public string EntityTitle        { get; set; } = string.Empty;
    public string ComplianceStatus   { get; set; } = string.Empty;
    public string? Notes             { get; set; }
}

public class SaveControlMappingRequest
{
    [Required] public int    RegulatoryControlId { get; set; }
    [Required] public string EntityType          { get; set; } = string.Empty;
    [Required] public int    EntityId            { get; set; }
    [Required] public string ComplianceStatus    { get; set; } = "NotAssessed";
    public string? Notes { get; set; }
}

public class UpdateMappingStatusRequest
{
    [Required] public string ComplianceStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

// ── Coverage ──────────────────────────────────────────────────────────────────

public class ComplianceCoverageDto
{
    public int    FrameworkId    { get; set; }
    public string FrameworkName  { get; set; } = string.Empty;
    public int    TotalControls  { get; set; }
    public int    Compliant      { get; set; }
    public int    PartiallyCompliant { get; set; }
    public int    NonCompliant   { get; set; }
    public int    NotAssessed    { get; set; }
    public double CoveragePercent { get; set; }  // (compliant + partial) / total
    public List<CoverageDomainDto> Domains { get; set; } = [];
}

public class CoverageDomainDto
{
    public string DomainAr      { get; set; } = string.Empty;
    public int    TotalControls { get; set; }
    public int    Compliant     { get; set; }
    public int    PartiallyCompliant { get; set; }
    public int    NonCompliant  { get; set; }
    public int    NotAssessed   { get; set; }
}
