using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public enum ComplianceStatus
{
    NotAssessed         = 0,
    Compliant           = 1,
    PartiallyCompliant  = 2,
    NonCompliant        = 3,
}

/// <summary>
/// Links a tenant's entity (ControlTest, Policy, Risk) to a RegulatoryControl.
/// Per-tenant data — one tenant's mappings are invisible to others.
/// </summary>
public class ControlMapping : AuditableEntity
{
    public int RegulatoryControlId { get; set; }
    public RegulatoryControl? RegulatoryControl { get; set; }

    /// <summary>ControlTest | Policy | Risk</summary>
    public string EntityType { get; set; } = string.Empty;
    public int    EntityId   { get; set; }

    public ComplianceStatus ComplianceStatus { get; set; } = ComplianceStatus.NotAssessed;

    public string? Notes { get; set; }
}
