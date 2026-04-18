using IGMS.Domain.Common;
using IGMS.Domain.Interfaces;

namespace IGMS.Domain.Entities;

/// <summary>
/// نشاط داخل مصفوفة RACI.
/// R  = Responsible  – من يُنفّذ الفعلاً.
/// A  = Accountable  – من يتحمل المسؤولية النهائية (مدير).
/// C/I يُخزَّنان في RaciParticipant.
/// </summary>
public class RaciActivity : AuditableEntity, ILocalizable
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    /// <summary>ترتيب العرض داخل المصفوفة</summary>
    public int DisplayOrder { get; set; } = 0;

    public int RaciMatrixId { get; set; }
    public RaciMatrix Matrix { get; set; } = null!;

    /// <summary>
    /// Accountable – صاحب القرار الوحيد (A واحد دائماً بحسب قاعدة RACI).
    /// R و C و I متعددون – مخزّنون في Participants.
    /// </summary>
    public int? AccountableUserId { get; set; }
    public UserProfile? AccountableUser { get; set; }

    /// <summary>R و C و I – أطراف متعددة لكل نشاط</summary>
    public ICollection<RaciParticipant> Participants { get; set; } = [];
}
