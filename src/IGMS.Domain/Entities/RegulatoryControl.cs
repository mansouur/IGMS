using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// A single control within a RegulatoryFramework.
/// Example: ISO 27001:2022 A.5.1 "Policies for information security".
/// </summary>
public class RegulatoryControl : AuditableEntity
{
    public int RegulatoryFrameworkId { get; set; }
    public RegulatoryFramework? Framework { get; set; }

    /// <summary>Control reference number: A.5.1 | CC-1.1 | NESA-TI-4.1</summary>
    public string ControlCode { get; set; } = string.Empty;

    /// <summary>High-level grouping within the framework (Domain/Section)</summary>
    public string DomainAr { get; set; } = string.Empty;
    public string DomainEn { get; set; } = string.Empty;

    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public ICollection<ControlMapping> Mappings { get; set; } = [];
}
