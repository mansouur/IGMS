namespace IGMS.Domain.Entities;

/// <summary>
/// مشارك R أو C أو I في نشاط RACI.
/// A (Accountable) يبقى FK مفرد على RaciActivity لأنه شخص واحد دائماً.
/// لا يرث AuditableEntity – جدول وصل خفيف.
/// </summary>
public class RaciParticipant
{
    public int RaciActivityId { get; set; }
    public RaciActivity Activity { get; set; } = null!;

    public int UserId { get; set; }
    public UserProfile User { get; set; } = null!;

    public ParticipantRole Role { get; set; }
}

public enum ParticipantRole
{
    Consulted   = 0,
    Informed    = 1,
    Responsible = 2,   // نُقل من FK مفرد إلى هنا لدعم تعدد المسؤولين
}
