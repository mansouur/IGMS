namespace IGMS.Domain.Entities;

/// <summary>
/// Many-to-many link between a Risk and the KPIs it may impact.
/// Soft-delete is NOT used here – rows are deleted physically.
/// </summary>
public class RiskKpiMapping
{
    public int  Id     { get; set; }
    public int  RiskId { get; set; }
    public Risk Risk   { get; set; } = null!;
    public int  KpiId  { get; set; }
    public Kpi  Kpi    { get; set; } = null!;
    public string? Notes { get; set; }
}
