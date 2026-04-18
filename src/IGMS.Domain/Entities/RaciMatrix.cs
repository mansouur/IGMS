using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// مصفوفة RACI – وثيقة على مستوى عملية أو مشروع.
/// تحتوي على أنشطة (RaciActivity) لكل منها توزيع R/A/C/I.
/// تستخدم TitleAr/TitleEn بدلاً من NameAr/NameEn لأنها عنوان وثيقة.
/// </summary>
public class RaciMatrix : AuditableEntity
{
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }

    /// <summary>القسم المالك لهذه المصفوفة</summary>
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public RaciStatus Status { get; set; } = RaciStatus.Draft;

    public int? ApprovedById { get; set; }
    public UserProfile? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<RaciActivity> Activities { get; set; } = [];
}

public enum RaciStatus
{
    Draft       = 0,
    UnderReview = 1,
    Approved    = 2,
    Archived    = 3
}
