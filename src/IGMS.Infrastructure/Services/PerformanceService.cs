using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class PerformanceService : IPerformanceService
{
    private readonly TenantDbContext _db;
    public PerformanceService(TenantDbContext db) => _db = db;

    // ── List ──────────────────────────────────────────────────────────────────

    public async Task<PagedResult<PerformanceReviewListDto>> GetPagedAsync(PerformanceQuery query)
    {
        var q = _db.PerformanceReviews
            .AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.Reviewer)
            .Include(r => r.Department)
            .Include(r => r.Goals.Where(g => !g.IsDeleted))
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(r => r.Employee.FullNameAr.Contains(query.Search) ||
                              r.Employee.FullNameEn != null && r.Employee.FullNameEn.Contains(query.Search));

        if (!string.IsNullOrEmpty(query.Status) &&
            Enum.TryParse<ReviewStatus>(query.Status, out var st))
            q = q.Where(r => r.Status == st);

        if (!string.IsNullOrEmpty(query.Period) &&
            Enum.TryParse<ReviewPeriod>(query.Period, out var pd))
            q = q.Where(r => r.Period == pd);

        if (query.Year.HasValue)
            q = q.Where(r => r.Year == query.Year.Value);

        if (query.EmployeeId.HasValue)
            q = q.Where(r => r.EmployeeId == query.EmployeeId.Value);

        if (query.DepartmentId.HasValue)
            q = q.Where(r => r.DepartmentId == query.DepartmentId.Value);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<PerformanceReviewListDto>
        {
            Items       = items.Select(MapList).ToList(),
            TotalCount  = total,
            CurrentPage = query.Page,
            PageSize    = query.PageSize,
        };
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    public async Task<PerformanceReviewDetailDto?> GetByIdAsync(int id)
    {
        var r = await LoadFull(id);
        return r == null ? null : MapDetail(r);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<PerformanceReviewDetailDto> CreateAsync(SavePerformanceReviewRequest req, int createdById)
    {
        if (!Enum.TryParse<ReviewPeriod>(req.Period, out var period))
            throw new InvalidOperationException("فترة التقييم غير صالحة.");

        var review = new PerformanceReview
        {
            EmployeeId           = req.EmployeeId,
            ReviewerId           = req.ReviewerId,
            Period               = period,
            Year                 = req.Year,
            DepartmentId         = req.DepartmentId,
            OverallRating        = req.OverallRating,
            StrengthsAr          = req.StrengthsAr,
            AreasForImprovementAr= req.AreasForImprovementAr,
            CommentsAr           = req.CommentsAr,
            EmployeeCommentsAr   = req.EmployeeCommentsAr,
            Status               = ReviewStatus.Draft,
            CreatedBy            = createdById.ToString(),
            CreatedAt            = DateTime.UtcNow,
        };

        AddGoals(review, req.Goals);
        _db.PerformanceReviews.Add(review);
        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(review.Id))!);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<PerformanceReviewDetailDto> UpdateAsync(int id, SavePerformanceReviewRequest req)
    {
        var review = await LoadFull(id)
            ?? throw new InvalidOperationException("التقييم غير موجود.");

        if (review.Status == ReviewStatus.Approved)
            throw new InvalidOperationException("لا يمكن تعديل تقييم معتمد.");

        if (!Enum.TryParse<ReviewPeriod>(req.Period, out var period))
            throw new InvalidOperationException("فترة التقييم غير صالحة.");

        review.EmployeeId            = req.EmployeeId;
        review.ReviewerId            = req.ReviewerId;
        review.Period                = period;
        review.Year                  = req.Year;
        review.DepartmentId          = req.DepartmentId;
        review.OverallRating         = req.OverallRating;
        review.StrengthsAr           = req.StrengthsAr;
        review.AreasForImprovementAr = req.AreasForImprovementAr;
        review.CommentsAr            = req.CommentsAr;
        review.EmployeeCommentsAr    = req.EmployeeCommentsAr;
        review.ModifiedAt            = DateTime.UtcNow;

        // Rebuild goals
        foreach (var g in review.Goals) { g.IsDeleted = true; g.DeletedAt = DateTime.UtcNow; }
        AddGoals(review, req.Goals);

        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(id))!);
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public async Task<PerformanceReviewDetailDto> SubmitAsync(int id)
    {
        var review = await LoadFull(id) ?? throw new InvalidOperationException("التقييم غير موجود.");
        if (review.Status != ReviewStatus.Draft)
            throw new InvalidOperationException("يمكن رفع المسودات فقط.");

        review.Status      = ReviewStatus.Submitted;
        review.SubmittedAt = DateTime.UtcNow;
        review.ModifiedAt  = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapDetail(review);
    }

    public async Task<PerformanceReviewDetailDto> ApproveAsync(int id)
    {
        var review = await LoadFull(id) ?? throw new InvalidOperationException("التقييم غير موجود.");
        if (review.Status != ReviewStatus.Submitted)
            throw new InvalidOperationException("يمكن اعتماد التقييمات المرفوعة فقط.");

        review.Status     = ReviewStatus.Approved;
        review.ApprovedAt = DateTime.UtcNow;
        review.ModifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapDetail(review);
    }

    public async Task<PerformanceReviewDetailDto> RejectAsync(int id, string? reason)
    {
        var review = await LoadFull(id) ?? throw new InvalidOperationException("التقييم غير موجود.");
        if (review.Status != ReviewStatus.Submitted)
            throw new InvalidOperationException("يمكن رفض التقييمات المرفوعة فقط.");

        review.Status       = ReviewStatus.Rejected;
        review.RejectReason = reason;
        review.ModifiedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapDetail(review);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task DeleteAsync(int id)
    {
        var review = await _db.PerformanceReviews.FindAsync(id)
            ?? throw new InvalidOperationException("التقييم غير موجود.");

        review.IsDeleted  = true;
        review.DeletedAt  = DateTime.UtcNow;
        review.ModifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Task<PerformanceReview?> LoadFull(int id) =>
        _db.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.Reviewer)
            .Include(r => r.Department)
            .Include(r => r.Goals.Where(g => !g.IsDeleted))
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

    private static void AddGoals(PerformanceReview review, List<SaveGoalRequest> goals)
    {
        foreach (var g in goals)
        {
            if (!Enum.TryParse<GoalStatus>(g.Status, out var gs)) gs = GoalStatus.Pending;
            review.Goals.Add(new PerformanceGoal
            {
                TitleAr       = g.TitleAr,
                DescriptionAr = g.DescriptionAr,
                Weight        = g.Weight,
                TargetValue   = g.TargetValue,
                ActualValue   = g.ActualValue,
                Rating        = g.Rating,
                Status        = gs,
                CreatedAt     = DateTime.UtcNow,
            });
        }
    }

    private static PerformanceReviewListDto MapList(PerformanceReview r) => new(
        r.Id,
        r.EmployeeId,
        r.Employee?.FullNameAr ?? r.Employee?.Username ?? "",
        r.ReviewerId,
        r.Reviewer?.FullNameAr ?? r.Reviewer?.Username ?? "",
        r.Period.ToString(),
        r.Year,
        r.Status.ToString(),
        r.OverallRating,
        r.Department?.NameAr,
        r.Goals.Count,
        r.CreatedAt
    );

    private static PerformanceReviewDetailDto MapDetail(PerformanceReview r) => new(
        r.Id,
        r.EmployeeId,
        r.Employee?.FullNameAr ?? r.Employee?.Username ?? "",
        r.ReviewerId,
        r.Reviewer?.FullNameAr ?? r.Reviewer?.Username ?? "",
        r.Period.ToString(),
        r.Year,
        r.Status.ToString(),
        r.OverallRating,
        r.StrengthsAr,
        r.AreasForImprovementAr,
        r.CommentsAr,
        r.EmployeeCommentsAr,
        r.RejectReason,
        r.SubmittedAt,
        r.ApprovedAt,
        r.Department?.NameAr,
        r.CreatedAt,
        r.Goals.Where(g => !g.IsDeleted).Select(g => new PerformanceGoalDto(
            g.Id, g.TitleAr, g.DescriptionAr,
            g.Weight, g.TargetValue, g.ActualValue, g.Rating,
            g.Status.ToString()
        )).ToList()
    );
}
