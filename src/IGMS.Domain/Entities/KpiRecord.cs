using System.ComponentModel.DataAnnotations.Schema;
using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// سجل تاريخي لقيم مؤشر الأداء عبر الفترات الزمنية.
/// يسمح برسم منحنى الاتجاه ومقارنة الهدف بالفعلي عبر الزمن.
/// </summary>
public class KpiRecord : AuditableEntity
{
    public int KpiId { get; set; }
    public Kpi Kpi   { get; set; } = null!;

    public int  Year    { get; set; }
    /// <summary>1–4 للربع السنوي، null للمؤشر السنوي</summary>
    public int? Quarter { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal TargetValue { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal ActualValue { get; set; }

    public string? Notes { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string   RecordedBy { get; set; } = string.Empty;
}
