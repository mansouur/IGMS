namespace IGMS.Application.Common.Models;

// ── Attendee DTO ──────────────────────────────────────────────────────────────

public class MeetingAttendeeDto
{
    public int     UserId       { get; set; }
    public string  FullNameAr   { get; set; } = string.Empty;
    public string? RoleInMeeting { get; set; }
    public bool    IsPresent    { get; set; }
}

// ── Action Item DTO ───────────────────────────────────────────────────────────

public class MeetingActionItemDto
{
    public int      Id             { get; set; }
    public string   TitleAr        { get; set; } = string.Empty;
    public string?  DescriptionAr  { get; set; }
    public string?  AssigneeName   { get; set; }
    public int?     AssignedToId   { get; set; }
    public DateTime? DueDate       { get; set; }
    public bool     IsCompleted    { get; set; }
    public DateTime? CompletedAt   { get; set; }
}

// ── List DTO ──────────────────────────────────────────────────────────────────

public class MeetingListDto
{
    public int      Id             { get; set; }
    public string   TitleAr        { get; set; } = string.Empty;
    public string?  TitleEn        { get; set; }
    public string   Type           { get; set; } = string.Empty;
    public string   Status         { get; set; } = string.Empty;
    public DateTime ScheduledAt    { get; set; }
    public string?  DepartmentName { get; set; }
    public string?  OrganizerName  { get; set; }
    public int      AttendeeCount  { get; set; }
    public int      ActionItemCount { get; set; }
    public int      PendingActions { get; set; }
}

// ── Detail DTO ────────────────────────────────────────────────────────────────

public class MeetingDetailDto : MeetingListDto
{
    public string?  Location       { get; set; }
    public string?  AgendaAr       { get; set; }
    public string?  MinutesAr      { get; set; }
    public string?  NotesAr        { get; set; }
    public DateTime? StartedAt     { get; set; }
    public DateTime? EndedAt       { get; set; }
    public int?     DepartmentId   { get; set; }
    public int?     OrganizerId    { get; set; }
    public DateTime CreatedAt      { get; set; }
    public List<MeetingAttendeeDto>  Attendees   { get; set; } = [];
    public List<MeetingActionItemDto> ActionItems { get; set; } = [];
}

// ── Save request ──────────────────────────────────────────────────────────────

public class SaveMeetingRequest
{
    public string   TitleAr     { get; set; } = string.Empty;
    public string?  TitleEn     { get; set; }
    public string   Type        { get; set; } = "Committee";
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
    public string?  Location    { get; set; }
    public string?  AgendaAr    { get; set; }
    public string?  NotesAr     { get; set; }
    public int?     DepartmentId { get; set; }
    public List<int> AttendeeIds { get; set; } = [];
}

// ── Minutes request ───────────────────────────────────────────────────────────

public class SaveMinutesRequest
{
    public string?  MinutesAr      { get; set; }
    public List<int> PresentIds    { get; set; } = [];  // من حضر فعلياً
    public List<SaveActionItemRequest> ActionItems { get; set; } = [];
}

public class SaveActionItemRequest
{
    public string   TitleAr       { get; set; } = string.Empty;
    public string?  DescriptionAr { get; set; }
    public int?     AssignedToId  { get; set; }
    public DateTime? DueDate      { get; set; }
}

// ── Query ─────────────────────────────────────────────────────────────────────

public class MeetingQuery
{
    public int     Page      { get; set; } = 1;
    public int     PageSize  { get; set; } = 20;
    public string? Search    { get; set; }
    public string? Type      { get; set; }
    public string? Status    { get; set; }
}
