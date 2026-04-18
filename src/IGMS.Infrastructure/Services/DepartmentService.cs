using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

/// <summary>
/// Implements IDepartmentService using TenantDbContext.
/// Level labels are NOT stored here – they come from tenant config in the UI.
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly TenantDbContext _db;

    public DepartmentService(TenantDbContext db)
    {
        _db = db;
    }

    // ── List (paged) ──────────────────────────────────────────────────────────

    public async Task<Result<PagedResult<DepartmentListDto>>> GetPagedAsync(DepartmentQuery query)
    {
        var q = _db.Departments
            .Include(d => d.Parent)
            .Include(d => d.Manager)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(d =>
                d.NameAr.Contains(query.Search) ||
                d.NameEn.Contains(query.Search) ||
                d.Code.Contains(query.Search));

        if (query.ParentId.HasValue)
            q = q.Where(d => d.ParentId == query.ParentId.Value);

        if (query.IsActive.HasValue)
            q = q.Where(d => d.IsActive == query.IsActive.Value);

        var total = await q.CountAsync();

        var items = await q
            .OrderBy(d => d.Level)
            .ThenBy(d => d.NameAr)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(d => new DepartmentListDto
            {
                Id            = d.Id,
                NameAr        = d.NameAr,
                NameEn        = d.NameEn,
                Code          = d.Code,
                Level         = d.Level,
                IsActive      = d.IsActive,
                ParentId      = d.ParentId,
                ParentNameAr  = d.Parent != null ? d.Parent.NameAr : null,
                ManagerNameAr = d.Manager != null ? d.Manager.FullNameAr : null,
                ChildCount    = d.Children.Count(c => !c.IsDeleted),
                MemberCount   = d.Members.Count(m => !m.IsDeleted),
                CreatedAt     = d.CreatedAt,
            })
            .ToListAsync();

        return Result<PagedResult<DepartmentListDto>>.Success(new PagedResult<DepartmentListDto>
        {
            Items       = items,
            TotalCount  = total,
            CurrentPage = query.Page,
            PageSize    = query.PageSize,
        });
    }

    // ── Tree ──────────────────────────────────────────────────────────────────

    public async Task<Result<List<DepartmentTreeDto>>> GetTreeAsync()
    {
        var all = await _db.Departments
            .AsNoTracking()
            .OrderBy(d => d.Level)
            .ThenBy(d => d.NameAr)
            .Select(d => new DepartmentTreeDto
            {
                Id       = d.Id,
                NameAr   = d.NameAr,
                NameEn   = d.NameEn,
                Code     = d.Code,
                Level    = d.Level,
                IsActive = d.IsActive,
                ParentId = d.ParentId,
            })
            .ToListAsync();

        var tree = BuildTree(all, null);
        return Result<List<DepartmentTreeDto>>.Success(tree);
    }

    private static List<DepartmentTreeDto> BuildTree(List<DepartmentTreeDto> all, int? parentId)
    {
        return all
            .Where(d => d.ParentId == parentId)
            .Select(d =>
            {
                d.Children = BuildTree(all, d.Id);
                return d;
            })
            .ToList();
    }

    // ── Get by ID ─────────────────────────────────────────────────────────────

    public async Task<Result<DepartmentDetailDto>> GetByIdAsync(int id)
    {
        var dept = await _db.Departments
            .Include(d => d.Parent)
            .Include(d => d.Manager)
            .Include(d => d.Children.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Manager)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dept is null)
            return Result<DepartmentDetailDto>.Failure("القسم غير موجود.");

        return Result<DepartmentDetailDto>.Success(MapDetail(dept));
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<Result<DepartmentDetailDto>> CreateAsync(CreateDepartmentRequest req, string createdBy)
    {
        // Validate unique code
        if (await _db.Departments.AnyAsync(d => d.Code == req.Code))
            return Result<DepartmentDetailDto>.Failure($"الرمز '{req.Code}' مستخدم بالفعل.");

        int level = 1;
        if (req.ParentId.HasValue)
        {
            var parent = await _db.Departments.FindAsync(req.ParentId.Value);
            if (parent is null)
                return Result<DepartmentDetailDto>.Failure("القسم الأعلى المحدد غير موجود.");
            level = parent.Level + 1;
        }

        // Validate manager if provided
        if (req.ManagerId.HasValue && !await _db.UserProfiles.AnyAsync(u => u.Id == req.ManagerId.Value))
            return Result<DepartmentDetailDto>.Failure("المدير المحدد غير موجود.");

        var dept = new Department
        {
            NameAr        = req.NameAr,
            NameEn        = req.NameEn,
            Code          = req.Code.ToUpperInvariant(),
            DescriptionAr = req.DescriptionAr,
            DescriptionEn = req.DescriptionEn,
            Level         = level,
            IsActive      = req.IsActive,
            ParentId      = req.ParentId,
            ManagerId     = req.ManagerId,
            CreatedBy     = createdBy,
            CreatedAt     = DateTime.UtcNow,
        };

        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(dept.Id);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<Result<DepartmentDetailDto>> UpdateAsync(UpdateDepartmentRequest req, string modifiedBy)
    {
        var dept = await _db.Departments.FindAsync(req.Id);
        if (dept is null)
            return Result<DepartmentDetailDto>.Failure("القسم غير موجود.");

        // Validate unique code (excluding self)
        if (await _db.Departments.AnyAsync(d => d.Code == req.Code && d.Id != req.Id))
            return Result<DepartmentDetailDto>.Failure($"الرمز '{req.Code}' مستخدم بالفعل.");

        // Prevent circular hierarchy
        if (req.ParentId.HasValue)
        {
            if (req.ParentId.Value == req.Id)
                return Result<DepartmentDetailDto>.Failure("لا يمكن تعيين القسم كأعلى لنفسه.");

            var parent = await _db.Departments.FindAsync(req.ParentId.Value);
            if (parent is null)
                return Result<DepartmentDetailDto>.Failure("القسم الأعلى المحدد غير موجود.");

            if (await IsDescendantAsync(req.ParentId.Value, req.Id))
                return Result<DepartmentDetailDto>.Failure("لا يمكن تعيين قسم فرعي كأعلى للقسم.");

            dept.Level = parent.Level + 1;
        }
        else
        {
            dept.Level = 1;
        }

        // Validate manager
        if (req.ManagerId.HasValue && !await _db.UserProfiles.AnyAsync(u => u.Id == req.ManagerId.Value))
            return Result<DepartmentDetailDto>.Failure("المدير المحدد غير موجود.");

        dept.NameAr        = req.NameAr;
        dept.NameEn        = req.NameEn;
        dept.Code          = req.Code.ToUpperInvariant();
        dept.DescriptionAr = req.DescriptionAr;
        dept.DescriptionEn = req.DescriptionEn;
        dept.IsActive      = req.IsActive;
        dept.ParentId      = req.ParentId;
        dept.ManagerId     = req.ManagerId;
        dept.ModifiedBy    = modifiedBy;
        dept.ModifiedAt    = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(dept.Id);
    }

    // ── Delete (soft) ─────────────────────────────────────────────────────────

    public async Task<Result<bool>> DeleteAsync(int id, string deletedBy)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept is null)
            return Result<bool>.Failure("القسم غير موجود.");

        if (await _db.Departments.AnyAsync(d => d.ParentId == id))
            return Result<bool>.Failure("لا يمكن حذف قسم يحتوي على أقسام فرعية. احذف الأقسام الفرعية أولاً.");

        if (await _db.UserProfiles.AnyAsync(u => u.DepartmentId == id))
            return Result<bool>.Failure("لا يمكن حذف قسم يضم موظفين. انقل الموظفين أولاً.");

        dept.IsDeleted  = true;
        dept.DeletedBy  = deletedBy;
        dept.DeletedAt  = DateTime.UtcNow;
        dept.ModifiedBy = deletedBy;
        dept.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Toggle Active ─────────────────────────────────────────────────────────

    public async Task<Result<bool>> SetActiveAsync(int id, bool isActive, string modifiedBy)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept is null)
            return Result<bool>.Failure("القسم غير موجود.");

        dept.IsActive   = isActive;
        dept.ModifiedBy = modifiedBy;
        dept.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<bool> IsDescendantAsync(int candidateParentId, int ancestorId)
    {
        // Walk up the tree from candidateParentId – if we reach ancestorId it's a cycle
        var current = candidateParentId;
        while (true)
        {
            var dept = await _db.Departments
                .AsNoTracking()
                .Select(d => new { d.Id, d.ParentId })
                .FirstOrDefaultAsync(d => d.Id == current);

            if (dept is null || dept.ParentId is null) return false;
            if (dept.ParentId.Value == ancestorId) return true;
            current = dept.ParentId.Value;
        }
    }

    private static DepartmentDetailDto MapDetail(Department d) => new()
    {
        Id            = d.Id,
        NameAr        = d.NameAr,
        NameEn        = d.NameEn,
        Code          = d.Code,
        Level         = d.Level,
        IsActive      = d.IsActive,
        ParentId      = d.ParentId,
        ParentNameAr  = d.Parent?.NameAr,
        ManagerNameAr = d.Manager?.FullNameAr,
        ChildCount    = d.Children.Count(c => !c.IsDeleted),
        MemberCount   = 0, // Members not loaded in detail (load separately if needed)
        CreatedAt     = d.CreatedAt,
        DescriptionAr = d.DescriptionAr,
        DescriptionEn = d.DescriptionEn,
        ManagerId     = d.ManagerId,
        ManagerNameEn = d.Manager?.FullNameEn,
        Children      = d.Children
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.NameAr)
            .Select(c => new DepartmentListDto
            {
                Id            = c.Id,
                NameAr        = c.NameAr,
                NameEn        = c.NameEn,
                Code          = c.Code,
                Level         = c.Level,
                IsActive      = c.IsActive,
                ParentId      = c.ParentId,
                ManagerNameAr = c.Manager?.FullNameAr,
                CreatedAt     = c.CreatedAt,
            })
            .ToList(),
    };
}
