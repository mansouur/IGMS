using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// A governance/compliance framework (ISO 27001, UAE NESA, COBIT 2019…).
/// Frameworks are seeded once — tenants cannot create their own.
/// </summary>
public class RegulatoryFramework : AuditableEntity
{
    /// <summary>Short machine code: ISO27001 | UAENESA | COBIT2019 | ADAA</summary>
    public string Code    { get; set; } = string.Empty;
    public string NameAr  { get; set; } = string.Empty;
    public string NameEn  { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }

    public bool IsActive { get; set; } = true;

    // ── Navigation ───────────────────────────────────────────────────────────
    public ICollection<RegulatoryControl> Controls { get; set; } = [];
}
