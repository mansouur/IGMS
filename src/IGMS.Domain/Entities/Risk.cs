using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public class Risk : AuditableEntity
{
    public string  TitleAr          { get; set; } = string.Empty;
    public string  TitleEn          { get; set; } = string.Empty;
    public string  Code             { get; set; } = string.Empty;
    public string? DescriptionAr    { get; set; }
    public string? MitigationPlanAr { get; set; }

    public RiskCategory Category   { get; set; } = RiskCategory.Operational;
    public RiskStatus   Status     { get; set; } = RiskStatus.Open;

    /// <summary>1–5</summary>
    public int Likelihood { get; set; } = 1;

    /// <summary>1–5</summary>
    public int Impact { get; set; } = 1;

    /// <summary>Computed: Likelihood × Impact</summary>
    public int RiskScore => Likelihood * Impact;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? OwnerId { get; set; }
    public UserProfile? Owner { get; set; }
}

public enum RiskCategory
{
    Operational = 0,
    Financial   = 1,
    IT          = 2,
    Legal       = 3,
    Strategic   = 4,
}

public enum RiskStatus
{
    Open      = 0,
    Mitigated = 1,
    Closed    = 2,
}
