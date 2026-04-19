using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public enum MeetingStatus { Scheduled, InProgress, Completed, Cancelled }
public enum MeetingType   { Board, Committee, Department, Emergency, Review }

/// <summary>
/// A governance committee meeting with agenda, attendees, minutes, and action items.
/// </summary>
public class Meeting : AuditableEntity
{
    public string  TitleAr       { get; set; } = string.Empty;
    public string? TitleEn       { get; set; }
    public MeetingType   Type    { get; set; } = MeetingType.Committee;
    public MeetingStatus Status  { get; set; } = MeetingStatus.Scheduled;

    public DateTime ScheduledAt  { get; set; }
    public DateTime? StartedAt   { get; set; }
    public DateTime? EndedAt     { get; set; }

    public string? Location      { get; set; }  // قاعة / رابط اجتماع
    public string? AgendaAr      { get; set; }  // JSON array of agenda items
    public string? MinutesAr     { get; set; }  // محضر الاجتماع
    public string? NotesAr       { get; set; }

    // ── Organizer ─────────────────────────────────────────────────────────────
    public int? OrganizerId     { get; set; }
    public UserProfile? Organizer { get; set; }

    // ── Department / Committee ────────────────────────────────────────────────
    public int?  DepartmentId   { get; set; }
    public Department? Department { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public ICollection<MeetingAttendee>  Attendees   { get; set; } = [];
    public ICollection<MeetingActionItem> ActionItems { get; set; } = [];
}

/// <summary>A person invited to or attending a meeting.</summary>
public class MeetingAttendee : AuditableEntity
{
    public int MeetingId { get; set; }
    public Meeting? Meeting { get; set; }

    public int UserId { get; set; }
    public UserProfile? User { get; set; }

    public bool  IsPresent  { get; set; } = false;  // حضر فعلياً؟
    public string? RoleInMeeting { get; set; }       // رئيس، مقرّر، عضو
}

/// <summary>An action item (decision/task) arising from a meeting.</summary>
public class MeetingActionItem : AuditableEntity
{
    public int MeetingId { get; set; }
    public Meeting? Meeting { get; set; }

    public string  TitleAr     { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }

    public int?    AssignedToId  { get; set; }
    public UserProfile? AssignedTo { get; set; }

    public DateTime? DueDate     { get; set; }
    public bool     IsCompleted  { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
}
