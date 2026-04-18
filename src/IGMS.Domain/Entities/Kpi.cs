using System.ComponentModel.DataAnnotations.Schema;
using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public class Kpi : AuditableEntity
{
    public string  TitleAr { get; set; } = string.Empty;
    public string  TitleEn { get; set; } = string.Empty;
    public string  Code    { get; set; } = string.Empty;
    public string? Unit    { get; set; }  // %, عدد, ساعة...

    [Column(TypeName = "decimal(18,4)")]
    public decimal TargetValue { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal ActualValue { get; set; }

    public int  Year    { get; set; } = DateTime.UtcNow.Year;
    /// <summary>1–4 للربع السنوي، null للمؤشر السنوي</summary>
    public int? Quarter { get; set; }

    public KpiStatus Status { get; set; } = KpiStatus.OnTrack;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? OwnerId { get; set; }
    public UserProfile? Owner { get; set; }

    public ICollection<KpiRecord> Records { get; set; } = [];
}

public enum KpiStatus
{
    OnTrack = 0,
    AtRisk  = 1,
    Behind  = 2,
}
