using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class MeetingService : IMeetingService
{
    private readonly TenantDbContext _db;
    public MeetingService(TenantDbContext db) => _db = db;

    // ── List ──────────────────────────────────────────────────────────────────

    public async Task<PagedResult<MeetingListDto>> GetPagedAsync(MeetingQuery query, int currentUserId)
    {
        var q = _db.Meetings
            .AsNoTracking()
            .Include(m => m.Department)
            .Include(m => m.Organizer)
            .Include(m => m.Attendees.Where(a => !a.IsDeleted))
            .Include(m => m.ActionItems.Where(ai => !ai.IsDeleted))
            .Where(m => !m.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(m => m.TitleAr.Contains(query.Search) ||
                              (m.TitleEn != null && m.TitleEn.Contains(query.Search)));

        if (!string.IsNullOrEmpty(query.Type) &&
            Enum.TryParse<MeetingType>(query.Type, out var mt))
            q = q.Where(m => m.Type == mt);

        if (!string.IsNullOrEmpty(query.Status) &&
            Enum.TryParse<MeetingStatus>(query.Status, out var ms))
            q = q.Where(m => m.Status == ms);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(m => m.ScheduledAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<MeetingListDto>
        {
            Items       = items.Select(MapList).ToList(),
            TotalCount  = total,
            CurrentPage = query.Page,
            PageSize    = query.PageSize,
        };
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    public async Task<MeetingDetailDto?> GetByIdAsync(int id)
    {
        var m = await LoadFull(id);
        return m == null ? null : MapDetail(m);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<MeetingDetailDto> CreateAsync(SaveMeetingRequest req, int organizerId)
    {
        if (!Enum.TryParse<MeetingType>(req.Type, out var type))
            throw new InvalidOperationException($"نوع اجتماع غير صالح: {req.Type}");

        var meeting = new Meeting
        {
            TitleAr      = req.TitleAr,
            TitleEn      = req.TitleEn,
            Type         = type,
            Status       = MeetingStatus.Scheduled,
            ScheduledAt  = req.ScheduledAt,
            Location     = req.Location,
            AgendaAr     = req.AgendaAr,
            NotesAr      = req.NotesAr,
            DepartmentId = req.DepartmentId,
            OrganizerId  = organizerId,
            CreatedAt    = DateTime.UtcNow,
            CreatedBy    = "api",
        };

        foreach (var uid in req.AttendeeIds.Distinct())
        {
            meeting.Attendees.Add(new MeetingAttendee
            {
                UserId    = uid,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "api",
            });
        }

        _db.Meetings.Add(meeting);
        await _db.SaveChangesAsync();

        return MapDetail((await LoadFull(meeting.Id))!);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<MeetingDetailDto> UpdateAsync(int id, SaveMeetingRequest req)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Attendees.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted)
            ?? throw new InvalidOperationException("الاجتماع غير موجود.");

        if (meeting.Status != MeetingStatus.Scheduled)
            throw new InvalidOperationException("لا يمكن تعديل اجتماع قيد التنفيذ أو منتهٍ.");

        if (!Enum.TryParse<MeetingType>(req.Type, out var type))
            throw new InvalidOperationException($"نوع اجتماع غير صالح: {req.Type}");

        meeting.TitleAr      = req.TitleAr;
        meeting.TitleEn      = req.TitleEn;
        meeting.Type         = type;
        meeting.ScheduledAt  = req.ScheduledAt;
        meeting.Location     = req.Location;
        meeting.AgendaAr     = req.AgendaAr;
        meeting.NotesAr      = req.NotesAr;
        meeting.DepartmentId = req.DepartmentId;
        meeting.ModifiedAt   = DateTime.UtcNow;

        // Rebuild attendees
        foreach (var a in meeting.Attendees.ToList())
        { a.IsDeleted = true; a.DeletedAt = DateTime.UtcNow; }

        foreach (var uid in req.AttendeeIds.Distinct())
        {
            meeting.Attendees.Add(new MeetingAttendee
            {
                UserId    = uid,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "api",
            });
        }

        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(id))!);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task DeleteAsync(int id)
    {
        var meeting = await _db.Meetings.FindAsync(id)
            ?? throw new InvalidOperationException("الاجتماع غير موجود.");
        meeting.IsDeleted = true;
        meeting.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Status transitions ────────────────────────────────────────────────────

    public async Task<MeetingDetailDto> StartAsync(int id)
    {
        var meeting = await _db.Meetings.FindAsync(id)
            ?? throw new InvalidOperationException("الاجتماع غير موجود.");
        if (meeting.Status != MeetingStatus.Scheduled)
            throw new InvalidOperationException("لا يمكن بدء هذا الاجتماع.");
        meeting.Status    = MeetingStatus.InProgress;
        meeting.StartedAt = DateTime.UtcNow;
        meeting.ModifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(id))!);
    }

    public async Task<MeetingDetailDto> CompleteAsync(int id, SaveMinutesRequest req)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Attendees.Where(a => !a.IsDeleted))
            .Include(m => m.ActionItems.Where(ai => !ai.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted)
            ?? throw new InvalidOperationException("الاجتماع غير موجود.");

        meeting.Status     = MeetingStatus.Completed;
        meeting.EndedAt    = DateTime.UtcNow;
        meeting.MinutesAr  = req.MinutesAr;
        meeting.ModifiedAt = DateTime.UtcNow;

        // Mark who attended
        foreach (var attendee in meeting.Attendees)
            attendee.IsPresent = req.PresentIds.Contains(attendee.UserId);

        // Soft-delete old action items and rebuild
        foreach (var ai in meeting.ActionItems.ToList())
        { ai.IsDeleted = true; ai.DeletedAt = DateTime.UtcNow; }

        foreach (var item in req.ActionItems)
        {
            meeting.ActionItems.Add(new MeetingActionItem
            {
                TitleAr       = item.TitleAr,
                DescriptionAr = item.DescriptionAr,
                AssignedToId  = item.AssignedToId,
                DueDate       = item.DueDate,
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = "api",
            });
        }

        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(id))!);
    }

    public async Task<MeetingDetailDto> CancelAsync(int id)
    {
        var meeting = await _db.Meetings.FindAsync(id)
            ?? throw new InvalidOperationException("الاجتماع غير موجود.");
        if (meeting.Status == MeetingStatus.Completed)
            throw new InvalidOperationException("لا يمكن إلغاء اجتماع منتهٍ.");
        meeting.Status     = MeetingStatus.Cancelled;
        meeting.ModifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(id))!);
    }

    // ── Action Item complete ──────────────────────────────────────────────────

    public async Task<MeetingActionItemDto> CompleteActionAsync(int meetingId, int actionId)
    {
        var action = await _db.MeetingActionItems
            .Include(ai => ai.AssignedTo)
            .FirstOrDefaultAsync(ai => ai.Id == actionId && ai.MeetingId == meetingId && !ai.IsDeleted)
            ?? throw new InvalidOperationException("نقطة العمل غير موجودة.");
        action.IsCompleted  = true;
        action.CompletedAt  = DateTime.UtcNow;
        action.ModifiedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapAction(action);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Meeting?> LoadFull(int id) =>
        await _db.Meetings
            .AsNoTracking()
            .Include(m => m.Department)
            .Include(m => m.Organizer)
            .Include(m => m.Attendees.Where(a => !a.IsDeleted)).ThenInclude(a => a.User)
            .Include(m => m.ActionItems.Where(ai => !ai.IsDeleted)).ThenInclude(ai => ai.AssignedTo)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

    private static MeetingListDto MapList(Meeting m) => new()
    {
        Id              = m.Id,
        TitleAr         = m.TitleAr,
        TitleEn         = m.TitleEn,
        Type            = m.Type.ToString(),
        Status          = m.Status.ToString(),
        ScheduledAt     = m.ScheduledAt,
        DepartmentName  = m.Department?.NameAr,
        OrganizerName   = m.Organizer?.FullNameAr,
        AttendeeCount   = m.Attendees.Count(a => !a.IsDeleted),
        ActionItemCount = m.ActionItems.Count(ai => !ai.IsDeleted),
        PendingActions  = m.ActionItems.Count(ai => !ai.IsDeleted && !ai.IsCompleted),
    };

    private static MeetingDetailDto MapDetail(Meeting m) => new()
    {
        Id              = m.Id,
        TitleAr         = m.TitleAr,
        TitleEn         = m.TitleEn,
        Type            = m.Type.ToString(),
        Status          = m.Status.ToString(),
        ScheduledAt     = m.ScheduledAt,
        StartedAt       = m.StartedAt,
        EndedAt         = m.EndedAt,
        Location        = m.Location,
        AgendaAr        = m.AgendaAr,
        MinutesAr       = m.MinutesAr,
        NotesAr         = m.NotesAr,
        DepartmentId    = m.DepartmentId,
        DepartmentName  = m.Department?.NameAr,
        OrganizerId     = m.OrganizerId,
        OrganizerName   = m.Organizer?.FullNameAr,
        AttendeeCount   = m.Attendees.Count(a => !a.IsDeleted),
        ActionItemCount = m.ActionItems.Count(ai => !ai.IsDeleted),
        PendingActions  = m.ActionItems.Count(ai => !ai.IsDeleted && !ai.IsCompleted),
        CreatedAt       = m.CreatedAt,
        Attendees       = m.Attendees.Where(a => !a.IsDeleted).Select(a => new MeetingAttendeeDto
        {
            UserId        = a.UserId,
            FullNameAr    = a.User?.FullNameAr ?? "",
            RoleInMeeting = a.RoleInMeeting,
            IsPresent     = a.IsPresent,
        }).ToList(),
        ActionItems = m.ActionItems.Where(ai => !ai.IsDeleted).Select(MapAction).ToList(),
    };

    private static MeetingActionItemDto MapAction(MeetingActionItem ai) => new()
    {
        Id            = ai.Id,
        TitleAr       = ai.TitleAr,
        DescriptionAr = ai.DescriptionAr,
        AssigneeName  = ai.AssignedTo?.FullNameAr,
        AssignedToId  = ai.AssignedToId,
        DueDate       = ai.DueDate,
        IsCompleted   = ai.IsCompleted,
        CompletedAt   = ai.CompletedAt,
    };
}
