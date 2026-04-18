using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TenantContext = IGMS.Application.Common.Models.TenantContext;

namespace IGMS.Infrastructure.Services;

public class PolicyService : IPolicyService
{
    private readonly TenantDbContext    _db;
    private readonly INotificationService _notify;
    private readonly TenantContext      _tenant;

    public PolicyService(TenantDbContext db, INotificationService notify, TenantContext tenant)
    {
        _db     = db;
        _notify = notify;
        _tenant = tenant;
    }

    // ── Shared filter builder ─────────────────────────────────────────────────
    private IQueryable<Policy> BuildQuery(PolicyQuery q)
    {
        var query = _db.Policies
            .Include(p => p.Department).Include(p => p.Owner).Include(p => p.Approver)
            .Where(p => !p.IsDeleted).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(p => p.TitleAr.Contains(q.Search) || p.Code.Contains(q.Search));
        if (q.Status.HasValue)   query = query.Where(p => p.Status   == q.Status.Value);
        if (q.Category.HasValue) query = query.Where(p => p.Category == q.Category.Value);

        return query;
    }

    // ── Paged list ────────────────────────────────────────────────────────────
    public async Task<Result<PagedResult<PolicyListDto>>> GetPagedAsync(PolicyQuery q)
    {
        var query = BuildQuery(q);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(ToListDto).ToListAsync();

        return Result<PagedResult<PolicyListDto>>.Success(
            PagedResult<PolicyListDto>.Create(items, total, q.Page, q.PageSize));
    }

    // ── Export (no pagination) ────────────────────────────────────────────────
    public async Task<byte[]> ExportAsync(PolicyQuery q)
    {
        var items = await BuildQuery(q)
            .OrderByDescending(p => p.CreatedAt)
            .Select(ToListDto).ToListAsync();

        var headers = new[]
        {
            "الرمز", "العنوان", "الفئة", "الحالة",
            "تاريخ السريان", "تاريخ الانتهاء",
            "القسم المالك", "المسؤول", "تاريخ الإنشاء",
        };

        string CatLabel(PolicyCategory c) => c switch
        {
            PolicyCategory.Governance  => "حوكمة",
            PolicyCategory.IT          => "تقنية",
            PolicyCategory.HR          => "موارد بشرية",
            PolicyCategory.Financial   => "مالي",
            PolicyCategory.Operations  => "تشغيلي",
            _                          => c.ToString(),
        };

        string StatusLabel(PolicyStatus s) => s switch
        {
            PolicyStatus.Draft    => "مسودة",
            PolicyStatus.Active   => "نشطة",
            PolicyStatus.Archived => "مؤرشفة",
            _                     => s.ToString(),
        };

        var rows = items.Select(p => new object?[]
        {
            p.Code, p.TitleAr, CatLabel(p.Category), StatusLabel(p.Status),
            p.EffectiveDate.HasValue ? p.EffectiveDate.Value.ToString("yyyy-MM-dd") : null,
            p.ExpiryDate.HasValue    ? p.ExpiryDate.Value.ToString("yyyy-MM-dd")    : null,
            p.DepartmentNameAr, p.OwnerNameAr,
            p.CreatedAt.ToString("yyyy-MM-dd"),
        });

        return ExcelExporter.Build("السياسات", headers, rows);
    }

    public async Task<Result<PolicyDetailDto>> GetByIdAsync(int id)
    {
        var p = await _db.Policies
            .Include(x => x.Department).Include(x => x.Owner).Include(x => x.Approver)
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (p is null) return Result<PolicyDetailDto>.Failure("السياسة غير موجودة.");
        return Result<PolicyDetailDto>.Success(Map(p));
    }

    public async Task<Result<PolicyDetailDto>> SaveAsync(SavePolicyRequest req, string by)
    {
        if (await _db.Policies.AnyAsync(p => p.Code == req.Code && p.Id != req.Id && !p.IsDeleted))
            return Result<PolicyDetailDto>.Failure("الرمز مستخدم مسبقاً.");

        Policy policy;
        if (req.Id == 0)
        {
            policy = new Policy { CreatedAt = DateTime.UtcNow, CreatedBy = by };
            _db.Policies.Add(policy);
        }
        else
        {
            policy = await _db.Policies.FirstOrDefaultAsync(p => p.Id == req.Id && !p.IsDeleted)
                     ?? throw new KeyNotFoundException();
            policy.ModifiedAt = DateTime.UtcNow; policy.ModifiedBy = by;
        }

        policy.TitleAr       = req.TitleAr;       policy.TitleEn  = req.TitleEn; policy.Code = req.Code;
        policy.DescriptionAr = req.DescriptionAr; policy.DescriptionEn = req.DescriptionEn;
        policy.Category      = req.Category;       policy.Status   = req.Status;
        policy.EffectiveDate = req.EffectiveDate;  policy.ExpiryDate = req.ExpiryDate;
        policy.DepartmentId  = req.DepartmentId;   policy.OwnerId  = req.OwnerId;

        // Handle approval when saving directly with Active status
        if (req.Status == PolicyStatus.Active)
        {
            if (req.ApproverId is null)
                return Result<PolicyDetailDto>.Failure("يجب تحديد المعتمد عند نشر السياسة.");

            if (!await _db.UserProfiles.AnyAsync(u => u.Id == req.ApproverId.Value && !u.IsDeleted))
                return Result<PolicyDetailDto>.Failure("المعتمد المحدد غير موجود.");

            policy.ApproverId = req.ApproverId;
            if (policy.ApprovedAt is null) // only set first time; preserve original timestamp on edits
                policy.ApprovedAt = DateTime.UtcNow;
        }
        else
        {
            policy.ApproverId = null;
            policy.ApprovedAt = null;
        }

        await _db.SaveChangesAsync();
        return await GetByIdAsync(policy.Id);
    }

    public async Task<Result<bool>> DeleteAsync(int id, string by)
    {
        var p = await _db.Policies.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (p is null) return Result<bool>.Failure("السياسة غير موجودة.");
        p.IsDeleted = true; p.ModifiedAt = DateTime.UtcNow; p.ModifiedBy = by;
        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> SetStatusAsync(int id, int status, string by, int? approverId = null)
    {
        if (!Enum.IsDefined(typeof(PolicyStatus), status))
            return Result<bool>.Failure("حالة غير صالحة.");

        var p = await _db.Policies.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (p is null) return Result<bool>.Failure("السياسة غير موجودة.");

        var newStatus = (PolicyStatus)status;

        // When publishing: require an approver and record approval timestamp
        if (newStatus == PolicyStatus.Active)
        {
            if (approverId is null)
                return Result<bool>.Failure("يجب تحديد المعتمد عند نشر السياسة.");

            if (!await _db.UserProfiles.AnyAsync(u => u.Id == approverId.Value && !u.IsDeleted))
                return Result<bool>.Failure("المعتمد المحدد غير موجود.");

            p.ApproverId = approverId;
            p.ApprovedAt = DateTime.UtcNow;
        }

        // When archiving or reverting to draft: clear approval data
        if (newStatus != PolicyStatus.Active)
        {
            p.ApproverId = null;
            p.ApprovedAt = null;
        }

        p.Status     = newStatus;
        p.ModifiedAt = DateTime.UtcNow;
        p.ModifiedBy = by;
        await _db.SaveChangesAsync();

        // Send email notification when publishing
        if (newStatus == PolicyStatus.Active)
        {
            var owner    = p.OwnerId.HasValue
                ? await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(u => u.Id == p.OwnerId.Value)
                : null;
            var approver = p.ApproverId.HasValue
                ? await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(u => u.Id == p.ApproverId.Value)
                : null;

            await _notify.PolicyPublishedAsync(
                policyTitle:      p.TitleAr,
                ownerEmail:       owner?.Email       ?? string.Empty,
                ownerName:        owner?.FullNameAr   ?? string.Empty,
                approverEmail:    approver?.Email     ?? string.Empty,
                approverName:     approver?.FullNameAr ?? string.Empty,
                organizationName: _tenant.Organization.NameAr);
        }

        return Result<bool>.Success(true);
    }

    // ── Renew ─────────────────────────────────────────────────────────────────
    public async Task<Result<PolicyDetailDto>> RenewAsync(int id, string by)
    {
        var original = await _db.Policies
            .Include(p => p.Department).Include(p => p.Owner).Include(p => p.Approver)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (original is null)
            return Result<PolicyDetailDto>.Failure("السياسة غير موجودة.");

        if (original.Status == PolicyStatus.Draft)
            return Result<PolicyDetailDto>.Failure("لا يمكن تجديد سياسة في مرحلة المسودة.");

        // Generate a unique code: original + "-R", then "-R2", "-R3"…
        var baseCode = original.Code + "-R";
        var newCode  = baseCode;
        var suffix   = 2;
        while (await _db.Policies.AnyAsync(p => p.Code == newCode && !p.IsDeleted))
            newCode = baseCode + suffix++;

        // New effective period: starts where old one ended (or today), expires +1 year
        var newEffective = original.ExpiryDate?.Date ?? DateTime.UtcNow.Date;
        var newExpiry    = newEffective.AddYears(1);

        var renewed = new Policy
        {
            TitleAr       = original.TitleAr,
            TitleEn       = original.TitleEn,
            Code          = newCode,
            DescriptionAr = original.DescriptionAr,
            DescriptionEn = original.DescriptionEn,
            Category      = original.Category,
            Status        = PolicyStatus.Draft,   // always starts as draft
            EffectiveDate = newEffective,
            ExpiryDate    = newExpiry,
            DepartmentId  = original.DepartmentId,
            OwnerId       = original.OwnerId,
            CreatedAt     = DateTime.UtcNow,
            CreatedBy     = by,
        };

        _db.Policies.Add(renewed);

        // Archive the original so it won't generate duplicate expiry alerts
        if (original.Status == PolicyStatus.Active)
        {
            original.Status     = PolicyStatus.Archived;
            original.ModifiedAt = DateTime.UtcNow;
            original.ModifiedBy = by;
        }

        await _db.SaveChangesAsync();
        return await GetByIdAsync(renewed.Id);
    }

    // ── Version History ───────────────────────────────────────────────────────
    public async Task<List<PolicyVersionDto>> GetVersionsAsync(int id)
    {
        var policy = await _db.Policies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (policy is null) return new();

        var baseCode = GetBaseCode(policy.Code);

        // All policies whose code IS the base OR starts with "baseCode-R"
        var allVersions = await _db.Policies
            .Include(p => p.Approver)
            .Where(p => !p.IsDeleted
                && (p.Code == baseCode || p.Code.StartsWith(baseCode + "-R")))
            .OrderBy(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return allVersions.Select((p, i) => new PolicyVersionDto
        {
            Id             = p.Id,
            Code           = p.Code,
            TitleAr        = p.TitleAr,
            Status         = p.Status,
            CreatedAt      = p.CreatedAt,
            ApprovedAt     = p.ApprovedAt,
            ApproverNameAr = p.Approver?.FullNameAr,
            EffectiveDate  = p.EffectiveDate,
            ExpiryDate     = p.ExpiryDate,
            IsCurrent      = p.Id == id,
            Version        = i + 1,
        }).ToList();
    }

    /// <summary>Extracts the root code by stripping "-R" or "-R{N}" suffixes.</summary>
    private static string GetBaseCode(string code)
    {
        var idx = code.LastIndexOf("-R", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return code;
        var afterR = code[(idx + 2)..];
        if (afterR.Length == 0 || (afterR.Length <= 3 && afterR.All(char.IsDigit)))
            return code[..idx];
        return code;
    }

    private static readonly System.Linq.Expressions.Expression<Func<Policy, PolicyListDto>> ToListDto = p =>
        new PolicyListDto
        {
            Id = p.Id, TitleAr = p.TitleAr, TitleEn = p.TitleEn, Code = p.Code,
            Category = p.Category, Status = p.Status,
            EffectiveDate = p.EffectiveDate, ExpiryDate = p.ExpiryDate,
            DepartmentNameAr = p.Department != null ? p.Department.NameAr : null,
            OwnerNameAr      = p.Owner      != null ? p.Owner.FullNameAr  : null,
            CreatedAt = p.CreatedAt,
        };

    private static PolicyDetailDto Map(Policy p) => new()
    {
        Id = p.Id, TitleAr = p.TitleAr, TitleEn = p.TitleEn, Code = p.Code,
        DescriptionAr = p.DescriptionAr, DescriptionEn = p.DescriptionEn,
        Category = p.Category, Status = p.Status,
        EffectiveDate = p.EffectiveDate, ExpiryDate = p.ExpiryDate,
        DepartmentId = p.DepartmentId, DepartmentNameAr = p.Department?.NameAr,
        OwnerId      = p.OwnerId,      OwnerNameAr      = p.Owner?.FullNameAr,
        ApproverId   = p.ApproverId,   ApproverNameAr   = p.Approver?.FullNameAr,
        ApprovedAt   = p.ApprovedAt,
        CreatedAt    = p.CreatedAt,
    };
}
