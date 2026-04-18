using System.ComponentModel.DataAnnotations;

namespace IGMS.Application.Common.Models;

public class KpiRecordDto
{
    public int      Id          { get; set; }
    public int      KpiId       { get; set; }
    public int      Year        { get; set; }
    public int?     Quarter     { get; set; }
    public decimal  TargetValue { get; set; }
    public decimal  ActualValue { get; set; }
    public string?  Notes       { get; set; }
    public DateTime RecordedAt  { get; set; }
    public string   RecordedBy  { get; set; } = string.Empty;

    /// <summary>نسبة الإنجاز = (الفعلي / الهدف) × 100</summary>
    public decimal AchievementPct => TargetValue == 0 ? 0
        : Math.Round(ActualValue / TargetValue * 100, 1);

    /// <summary>تسمية الفترة للعرض في الرسم البياني: "Q1 2025" أو "2025"</summary>
    public string PeriodLabel => Quarter.HasValue ? $"Q{Quarter} {Year}" : Year.ToString();
}

public class AddKpiRecordRequest
{
    [Required] public int  KpiId       { get; set; }
    [Required] public int  Year        { get; set; } = DateTime.UtcNow.Year;
    [Range(1, 4)] public int? Quarter  { get; set; }
    public decimal TargetValue         { get; set; }
    public decimal ActualValue         { get; set; }
    public string? Notes               { get; set; }
}
