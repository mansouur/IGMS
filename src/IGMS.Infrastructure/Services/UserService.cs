using BCrypt.Net;
using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly TenantDbContext _db;

    public UserService(TenantDbContext db) => _db = db;

    // ── List (paged) ──────────────────────────────────────────────────────────

    public async Task<Result<PagedResult<UserListDto>>> GetPagedAsync(UserQuery query)
    {
        var q = _db.UserProfiles
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(u =>
                u.FullNameAr.Contains(query.Search) ||
                u.FullNameEn.Contains(query.Search) ||
                u.Username.Contains(query.Search)   ||
                u.Email.Contains(query.Search));

        if (query.DepartmentId.HasValue)
            q = q.Where(u => u.DepartmentId == query.DepartmentId.Value);

        if (query.IsActive.HasValue)
            q = q.Where(u => u.IsActive == query.IsActive.Value);

        var total = await q.CountAsync();

        var items = await q
            .OrderBy(u => u.FullNameAr)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new UserListDto
            {
                Id               = u.Id,
                Username         = u.Username,
                FullNameAr       = u.FullNameAr,
                FullNameEn       = u.FullNameEn,
                Email            = u.Email,
                IsActive         = u.IsActive,
                DepartmentId     = u.DepartmentId,
                DepartmentNameAr = u.Department != null ? u.Department.NameAr : null,
                Roles            = u.UserRoles.Select(ur => ur.Role.NameAr).ToList(),
                CreatedAt        = u.CreatedAt,
                LastLoginAt      = u.LastLoginAt,
            })
            .ToListAsync();

        return Result<PagedResult<UserListDto>>.Success(new PagedResult<UserListDto>
        {
            Items       = items,
            TotalCount  = total,
            CurrentPage = query.Page,
            PageSize    = query.PageSize,
        });
    }

    // ── Lookup (UserIdPicker) ─────────────────────────────────────────────────

    public async Task<Result<List<UserListDto>>> GetLookupAsync()
    {
        var items = await _db.UserProfiles
            .Where(u => !u.IsDeleted && u.IsActive)
            .OrderBy(u => u.FullNameAr)
            .Select(u => new UserListDto
            {
                Id         = u.Id,
                Username   = u.Username,
                FullNameAr = u.FullNameAr,
                FullNameEn = u.FullNameEn,
                Email      = u.Email,
            })
            .AsNoTracking()
            .ToListAsync();

        return Result<List<UserListDto>>.Success(items);
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    public async Task<Result<UserDetailDto>> GetByIdAsync(int id)
    {
        var u = await _db.UserProfiles
            .Include(x => x.Department)
            .Include(x => x.UserRoles).ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (u is null)
            return Result<UserDetailDto>.Failure("المستخدم غير موجود.");

        return Result<UserDetailDto>.Success(MapDetail(u));
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<Result<UserDetailDto>> CreateAsync(CreateUserRequest req, string createdBy)
    {
        if (await _db.UserProfiles.AnyAsync(u => u.Username == req.Username && !u.IsDeleted))
            return Result<UserDetailDto>.Failure("اسم المستخدم مستخدم مسبقاً.");

        if (await _db.UserProfiles.AnyAsync(u => u.Email == req.Email && !u.IsDeleted))
            return Result<UserDetailDto>.Failure("البريد الإلكتروني مستخدم مسبقاً.");

        if (req.DepartmentId.HasValue &&
            !await _db.Departments.AnyAsync(d => d.Id == req.DepartmentId.Value && !d.IsDeleted))
            return Result<UserDetailDto>.Failure("القسم المحدد غير موجود.");

        var user = new UserProfile
        {
            Username     = req.Username,
            FullNameAr   = req.FullNameAr,
            FullNameEn   = req.FullNameEn,
            Email        = req.Email,
            PhoneNumber  = req.PhoneNumber,
            EmiratesId   = req.EmiratesId?.Trim() is { Length: > 0 } eid ? eid : null,
            DepartmentId = req.DepartmentId,
            IsActive     = req.IsActive,
            PasswordHash = !string.IsNullOrWhiteSpace(req.Password)
                ? BCrypt.Net.BCrypt.HashPassword(req.Password, 11)
                : null,
            CreatedAt    = DateTime.UtcNow,
            CreatedBy    = createdBy,
        };

        _db.UserProfiles.Add(user);
        await _db.SaveChangesAsync();

        await SyncRolesAsync(user.Id, req.RoleIds, createdBy);

        return await GetByIdAsync(user.Id);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<Result<UserDetailDto>> UpdateAsync(UpdateUserRequest req, string modifiedBy)
    {
        var user = await _db.UserProfiles
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == req.Id && !u.IsDeleted);

        if (user is null)
            return Result<UserDetailDto>.Failure("المستخدم غير موجود.");

        if (await _db.UserProfiles.AnyAsync(u => u.Email == req.Email && u.Id != req.Id && !u.IsDeleted))
            return Result<UserDetailDto>.Failure("البريد الإلكتروني مستخدم مسبقاً.");

        if (req.DepartmentId.HasValue &&
            !await _db.Departments.AnyAsync(d => d.Id == req.DepartmentId.Value && !d.IsDeleted))
            return Result<UserDetailDto>.Failure("القسم المحدد غير موجود.");

        user.FullNameAr   = req.FullNameAr;
        user.FullNameEn   = req.FullNameEn;
        user.Email        = req.Email;
        user.PhoneNumber  = req.PhoneNumber;
        user.EmiratesId   = req.EmiratesId?.Trim() is { Length: > 0 } eid ? eid : null;
        user.DepartmentId = req.DepartmentId;
        user.IsActive     = req.IsActive;
        user.ModifiedAt   = DateTime.UtcNow;
        user.ModifiedBy   = modifiedBy;

        await _db.SaveChangesAsync();
        await SyncRolesAsync(user.Id, req.RoleIds, modifiedBy);

        return await GetByIdAsync(user.Id);
    }

    // ── Delete (Soft) ─────────────────────────────────────────────────────────

    public async Task<Result<bool>> DeleteAsync(int id, string deletedBy)
    {
        var user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user is null)
            return Result<bool>.Failure("المستخدم غير موجود.");

        user.IsDeleted  = true;
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = deletedBy;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ── SetActive ─────────────────────────────────────────────────────────────

    public async Task<Result<bool>> SetActiveAsync(int id, bool isActive, string modifiedBy)
    {
        var user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user is null)
            return Result<bool>.Failure("المستخدم غير موجود.");

        user.IsActive   = isActive;
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = modifiedBy;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ── Export (no pagination) ────────────────────────────────────────────────

    public async Task<byte[]> ExportAsync(UserQuery q)
    {
        var query = _db.UserProfiles
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(u =>
                u.FullNameAr.Contains(q.Search) ||
                u.Username.Contains(q.Search)   ||
                u.Email.Contains(q.Search));
        if (q.DepartmentId.HasValue) query = query.Where(u => u.DepartmentId == q.DepartmentId.Value);
        if (q.IsActive.HasValue)     query = query.Where(u => u.IsActive     == q.IsActive.Value);

        var items = await query
            .OrderBy(u => u.FullNameAr)
            .Select(u => new
            {
                u.Username, u.FullNameAr, u.FullNameEn, u.Email, u.IsActive,
                DeptName = u.Department != null ? u.Department.NameAr : null,
                Roles    = u.UserRoles.Select(ur => ur.Role.NameAr).ToList(),
                u.CreatedAt, u.LastLoginAt,
            })
            .ToListAsync();

        var headers = new[]
        {
            "اسم المستخدم", "الاسم بالعربية", "الاسم بالإنجليزية",
            "البريد الإلكتروني", "الحالة", "القسم", "الأدوار",
            "تاريخ الإنشاء", "آخر دخول",
        };

        var rows = items.Select(u => new object?[]
        {
            u.Username, u.FullNameAr, u.FullNameEn, u.Email,
            u.IsActive ? "نشط" : "معطل",
            u.DeptName,
            string.Join(" / ", u.Roles),
            u.CreatedAt.ToString("yyyy-MM-dd"),
            u.LastLoginAt.HasValue ? u.LastLoginAt.Value.ToString("yyyy-MM-dd") : null,
        });

        return ExcelExporter.Build("المستخدمون", headers, rows);
    }

    // ── ChangePassword ────────────────────────────────────────────────────────

    public async Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user is null)
            return Result<bool>.Failure("المستخدم غير موجود.");

        if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            return Result<bool>.Failure("كلمة المرور الحالية غير صحيحة.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, 11);
        user.ModifiedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task SyncRolesAsync(int userId, List<int> roleIds, string assignedBy)
    {
        var existing = await _db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
        _db.UserRoles.RemoveRange(existing);

        foreach (var roleId in roleIds.Distinct())
        {
            _db.UserRoles.Add(new UserRole
            {
                UserId     = userId,
                RoleId     = roleId,
                AssignedBy = assignedBy,
                AssignedAt = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync();
    }

    private static UserDetailDto MapDetail(UserProfile u) => new()
    {
        Id               = u.Id,
        Username         = u.Username,
        FullNameAr       = u.FullNameAr,
        FullNameEn       = u.FullNameEn,
        Email            = u.Email,
        PhoneNumber      = u.PhoneNumber,
        IsActive         = u.IsActive,
        DepartmentId     = u.DepartmentId,
        DepartmentNameAr = u.Department?.NameAr,
        Roles            = u.UserRoles.Select(ur => ur.Role.NameAr).ToList(),
        AdObjectId       = u.AdObjectId,
        UaePassSub       = u.UaePassSub,
        EmiratesId       = u.EmiratesId,
        CreatedAt        = u.CreatedAt,
        LastLoginAt      = u.LastLoginAt,
    };
}
