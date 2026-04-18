using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

/// <summary>
/// Implements IRaciService using TenantDbContext directly (Infrastructure layer).
/// Pagination, filtering, and eager loading all handled here.
/// </summary>
public class RaciService : IRaciService
{
    private readonly TenantDbContext _db;

    public RaciService(TenantDbContext db)
    {
        _db = db;
    }

    // ── List (paged) ──────────────────────────────────────────────────────────

    public async Task<Result<PagedResult<RaciMatrixListDto>>> GetPagedAsync(RaciMatrixQuery query)
    {
        var q = _db.RaciMatrices
            .Include(r => r.Department)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(r => r.TitleAr.Contains(query.Search) || r.TitleEn.Contains(query.Search));

        if (query.Status.HasValue)
            q = q.Where(r => r.Status == query.Status.Value);

        if (query.DepartmentId.HasValue)
            q = q.Where(r => r.DepartmentId == query.DepartmentId.Value);

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new RaciMatrixListDto
            {
                Id            = r.Id,
                TitleAr       = r.TitleAr,
                TitleEn       = r.TitleEn,
                DepartmentAr  = r.Department != null ? r.Department.NameAr : null,
                DepartmentEn  = r.Department != null ? r.Department.NameEn : null,
                Status        = r.Status,
                StatusLabel   = GetStatusLabel(r.Status),
                ActivityCount = r.Activities.Count(a => !a.IsDeleted),
                CreatedBy     = r.CreatedBy,
                CreatedAt     = r.CreatedAt,
                ApprovedAt    = r.ApprovedAt
            })
            .ToListAsync();

        return Result<PagedResult<RaciMatrixListDto>>.Success(new PagedResult<RaciMatrixListDto>
        {
            Items        = items,
            TotalCount   = total,
            CurrentPage  = query.Page,
            PageSize     = query.PageSize
        });
    }

    // ── Get by ID ─────────────────────────────────────────────────────────────

    public async Task<Result<RaciMatrixDetailDto>> GetByIdAsync(int id)
    {
        var matrix = await _db.RaciMatrices
            .Include(r => r.Department)
            .Include(r => r.ApprovedBy)
            .Include(r => r.Activities.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.AccountableUser)
            .Include(r => r.Activities.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.Participants)
                    .ThenInclude(p => p.User)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (matrix is null)
            return Result<RaciMatrixDetailDto>.Failure("مصفوفة RACI غير موجودة.");

        return Result<RaciMatrixDetailDto>.Success(MapToDetail(matrix));
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<Result<RaciMatrixDetailDto>> CreateAsync(CreateRaciMatrixRequest request, string createdBy)
    {
        var validationError = await ValidateUserIdsAsync(request.Activities);
        if (validationError is not null)
            return Result<RaciMatrixDetailDto>.Failure(validationError);

        var matrix = new RaciMatrix
        {
            TitleAr       = request.TitleAr,
            TitleEn       = request.TitleEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            DepartmentId  = request.DepartmentId,
            Status        = RaciStatus.Draft,
            CreatedBy     = createdBy,
            CreatedAt     = DateTime.UtcNow,
        };

        matrix.Activities = BuildActivities(request.Activities, createdBy);

        _db.RaciMatrices.Add(matrix);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(matrix.Id);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<Result<RaciMatrixDetailDto>> UpdateAsync(UpdateRaciMatrixRequest request, string modifiedBy)
    {
        var validationError = await ValidateUserIdsAsync(request.Activities);
        if (validationError is not null)
            return Result<RaciMatrixDetailDto>.Failure(validationError);

        // IgnoreQueryFilters: نحتاج رؤية الأنشطة المحذوفة سابقاً لتجنب تضارب الـ change tracker
        var matrix = await _db.RaciMatrices
            .IgnoreQueryFilters()
            .Include(r => r.Activities)
                .ThenInclude(a => a.Participants)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted);

        if (matrix is null)
            return Result<RaciMatrixDetailDto>.Failure("مصفوفة RACI غير موجودة.");

        if (matrix.Status == RaciStatus.Approved)
            return Result<RaciMatrixDetailDto>.Failure("لا يمكن تعديل مصفوفة معتمدة.");

        matrix.TitleAr        = request.TitleAr;
        matrix.TitleEn        = request.TitleEn;
        matrix.DescriptionAr  = request.DescriptionAr;
        matrix.DescriptionEn  = request.DescriptionEn;
        matrix.DepartmentId   = request.DepartmentId;
        matrix.ModifiedBy     = modifiedBy;
        matrix.ModifiedAt     = DateTime.UtcNow;

        // حذف الـ participants أولاً ثم soft-delete للأنشطة
        foreach (var activity in matrix.Activities)
        {
            _db.RaciParticipants.RemoveRange(activity.Participants);
            activity.IsDeleted  = true;
            activity.DeletedAt  = DateTime.UtcNow;
            activity.DeletedBy  = modifiedBy;
        }

        // إضافة الأنشطة الجديدة مع participants
        var newActivities = BuildActivities(request.Activities, modifiedBy);
        _db.RaciActivities.AddRange(newActivities.Select(a => { a.RaciMatrixId = matrix.Id; return a; }));

        await _db.SaveChangesAsync();
        return await GetByIdAsync(matrix.Id);
    }

    // ── Delete (soft) ─────────────────────────────────────────────────────────

    public async Task<Result<bool>> DeleteAsync(int id, string deletedBy)
    {
        var matrix = await _db.RaciMatrices.FindAsync(id);

        if (matrix is null)
            return Result<bool>.Failure("مصفوفة RACI غير موجودة.");

        if (matrix.Status == RaciStatus.Approved)
            return Result<bool>.Failure("لا يمكن حذف مصفوفة معتمدة.");

        matrix.IsDeleted  = true;
        matrix.DeletedAt  = DateTime.UtcNow;
        matrix.DeletedBy  = deletedBy;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ── Submit for Review ─────────────────────────────────────────────────────

    public async Task<Result<bool>> SubmitForReviewAsync(int id, string modifiedBy)
    {
        var matrix = await _db.RaciMatrices.FindAsync(id);

        if (matrix is null)
            return Result<bool>.Failure("مصفوفة RACI غير موجودة.");

        if (matrix.Status != RaciStatus.Draft)
            return Result<bool>.Failure("يمكن إرسال المصفوفات بحالة 'مسودة' فقط.");

        matrix.Status     = RaciStatus.UnderReview;
        matrix.ModifiedBy = modifiedBy;
        matrix.ModifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ── Approve ───────────────────────────────────────────────────────────────

    public async Task<Result<RaciMatrixDetailDto>> ApproveAsync(int id, string approvedByUsername, int approvedByUserId)
    {
        var matrix = await _db.RaciMatrices.FindAsync(id);

        if (matrix is null)
            return Result<RaciMatrixDetailDto>.Failure("مصفوفة RACI غير موجودة.");

        if (matrix.Status != RaciStatus.UnderReview)
            return Result<RaciMatrixDetailDto>.Failure("يمكن اعتماد المصفوفات بحالة 'قيد المراجعة' فقط.");

        matrix.Status        = RaciStatus.Approved;
        matrix.ApprovedById  = approvedByUserId;
        matrix.ApprovedAt    = DateTime.UtcNow;
        matrix.ModifiedBy    = approvedByUsername;
        matrix.ModifiedAt    = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetByIdAsync(matrix.Id);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// يتحقق أن كل user IDs في الطلب موجودة فعلاً في UserProfiles.
    /// يُرجع رسالة خطأ إذا وجد ID غير موجود، أو null إذا كل شيء سليم.
    /// </summary>
    private async Task<string?> ValidateUserIdsAsync(List<CreateRaciActivityRequest> activities)
    {
        var allIds = activities
            .SelectMany(a =>
                a.ResponsibleUserIds
                 .Concat(a.ConsultedUserIds)
                 .Concat(a.InformedUserIds)
                 .Concat(a.AccountableUserId.HasValue ? [a.AccountableUserId.Value] : []))
            .Distinct()
            .ToList();

        if (allIds.Count == 0) return null;

        var existingIds = await _db.UserProfiles
            .IgnoreQueryFilters()
            .Where(u => allIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync();

        var missing = allIds.Except(existingIds).ToList();
        if (missing.Count > 0)
            return $"المستخدمون التاليون غير موجودون: {string.Join(", ", missing)}. تحقق من معرّفات المستخدمين.";

        return null;
    }

    private static List<RaciActivity> BuildActivities(
        List<CreateRaciActivityRequest> requests, string createdBy) =>
        requests.Select(r => BuildActivity(r, createdBy)).ToList();

    private static RaciActivity BuildActivity(CreateRaciActivityRequest r, string createdBy)
    {
        var activity = new RaciActivity
        {
            NameAr            = r.NameAr,
            NameEn            = r.NameEn,
            DisplayOrder      = r.DisplayOrder,
            AccountableUserId = r.AccountableUserId,
            CreatedBy         = createdBy,
            CreatedAt         = DateTime.UtcNow,
        };

        foreach (var uid in r.ResponsibleUserIds.Distinct())
            activity.Participants.Add(new RaciParticipant { UserId = uid, Role = ParticipantRole.Responsible });

        foreach (var uid in r.ConsultedUserIds.Distinct())
            activity.Participants.Add(new RaciParticipant { UserId = uid, Role = ParticipantRole.Consulted });

        foreach (var uid in r.InformedUserIds.Distinct())
            activity.Participants.Add(new RaciParticipant { UserId = uid, Role = ParticipantRole.Informed });

        return activity;
    }

    private static RaciMatrixDetailDto MapToDetail(RaciMatrix r) => new()
    {
        Id            = r.Id,
        TitleAr       = r.TitleAr,
        TitleEn       = r.TitleEn,
        DescriptionAr = r.DescriptionAr,
        DescriptionEn = r.DescriptionEn,
        DepartmentAr  = r.Department?.NameAr,
        DepartmentEn  = r.Department?.NameEn,
        Status        = r.Status,
        StatusLabel   = GetStatusLabel(r.Status),
        ActivityCount = r.Activities.Count(a => !a.IsDeleted),
        CreatedBy     = r.CreatedBy,
        CreatedAt     = r.CreatedAt,
        ApprovedAt    = r.ApprovedAt,
        ApprovedByName = r.ApprovedBy is not null
            ? $"{r.ApprovedBy.FullNameAr}"
            : null,
        Activities = r.Activities
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.DisplayOrder)
            .Select(MapActivity)
            .ToList()
    };

    private static RaciActivityDto MapActivity(RaciActivity a) => new()
    {
        Id            = a.Id,
        NameAr        = a.NameAr,
        NameEn        = a.NameEn,
        DisplayOrder  = a.DisplayOrder,
        Responsible   = a.Participants.Where(p => p.Role == ParticipantRole.Responsible).Select(p => MapUser(p.User)).ToList(),
        Accountable   = a.AccountableUser is not null ? MapUser(a.AccountableUser) : null,
        Consulted     = a.Participants.Where(p => p.Role == ParticipantRole.Consulted).Select(p => MapUser(p.User)).ToList(),
        Informed      = a.Participants.Where(p => p.Role == ParticipantRole.Informed).Select(p => MapUser(p.User)).ToList(),
    };

    private static UserRefDto MapUser(UserProfile u) => new()
    {
        Id         = u.Id,
        FullNameAr = u.FullNameAr,
        FullNameEn = u.FullNameEn,
        Username   = u.Username
    };

    private static string GetStatusLabel(RaciStatus status) => status switch
    {
        RaciStatus.Draft       => "مسودة",
        RaciStatus.UnderReview => "قيد المراجعة",
        RaciStatus.Approved    => "معتمدة",
        RaciStatus.Archived    => "مؤرشفة",
        _                      => "غير محدد"
    };
}
