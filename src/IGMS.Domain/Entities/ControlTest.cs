using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// اختبار ضابط رقابي — يُثبت وجود ضابط وفعاليته بالأدلة.
/// يمكن ربطه بسياسة أو مخاطرة.
/// </summary>
public class ControlTest : AuditableEntity
{
    public string  TitleAr       { get; set; } = string.Empty;
    public string? TitleEn       { get; set; }
    public string  Code          { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }

    /// <summary>"Policy" أو "Risk"</summary>
    public string EntityType { get; set; } = "Policy";

    /// <summary>Id السياسة أو المخاطرة المرتبطة</summary>
    public int EntityId { get; set; }

    /// <summary>من أجرى الاختبار</summary>
    public int?          TestedById { get; set; }
    public UserProfile?  TestedBy   { get; set; }
    public DateTime?     TestedAt   { get; set; }

    /// <summary>موعد الاختبار التالي المقرر</summary>
    public DateTime? NextTestDate { get; set; }

    public ControlEffectiveness Effectiveness { get; set; } = ControlEffectiveness.NotTested;

    /// <summary>النتائج والملاحظات</summary>
    public string? FindingsAr { get; set; }

    public ICollection<ControlEvidence> Evidences { get; set; } = [];
}

public enum ControlEffectiveness
{
    NotTested          = 0,
    Effective          = 1,
    PartiallyEffective = 2,
    Ineffective        = 3,
}
