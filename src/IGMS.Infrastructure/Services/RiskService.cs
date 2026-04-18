using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class RiskService : IRiskService
{
    private readonly TenantDbContext _db;
    public RiskService(TenantDbContext db) => _db = db;

    // ── Shared filter builder ─────────────────────────────────────────────────
    private IQueryable<Risk> BuildQuery(RiskQuery q)
    {
        var query = _db.Risks
            .Include(r => r.Department).Include(r => r.Owner)
            .Where(r => !r.IsDeleted).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(r => r.TitleAr.Contains(q.Search) || r.Code.Contains(q.Search));
        if (q.Status.HasValue)   query = query.Where(r => r.Status   == q.Status.Value);
        if (q.Category.HasValue) query = query.Where(r => r.Category == q.Category.Value);

        return query;
    }

    // ── Paged list ────────────────────────────────────────────────────────────
    public async Task<Result<PagedResult<RiskListDto>>> GetPagedAsync(RiskQuery q)
    {
        var query = BuildQuery(q);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(ToListDto)
            .ToListAsync();

        return Result<PagedResult<RiskListDto>>.Success(
            PagedResult<RiskListDto>.Create(items, total, q.Page, q.PageSize));
    }

    // ── Export (no pagination) ────────────────────────────────────────────────
    public async Task<byte[]> ExportAsync(RiskQuery q)
    {
        var items = await BuildQuery(q)
            .OrderByDescending(r => r.CreatedAt)
            .Select(ToListDto)
            .ToListAsync();

        var headers = new[]
        {
            "الرمز", "العنوان", "الفئة", "الحالة",
            "الاحتمالية", "التأثير", "درجة الخطر",
            "القسم", "المسؤول", "تاريخ الإنشاء",
        };

        string CatLabel(RiskCategory c) => c switch
        {
            RiskCategory.Operational  => "تشغيلي",
            RiskCategory.Financial    => "مالي",
            RiskCategory.IT           => "تقنية",
            RiskCategory.Legal        => "قانوني",
            RiskCategory.Strategic    => "استراتيجي",
            _                         => c.ToString(),
        };

        string StatusLabel(RiskStatus s) => s switch
        {
            RiskStatus.Open      => "مفتوحة",
            RiskStatus.Mitigated => "مُخففة",
            RiskStatus.Closed    => "مغلقة",
            _                    => s.ToString(),
        };

        var rows = items.Select(r => new object?[]
        {
            r.Code, r.TitleAr, CatLabel(r.Category), StatusLabel(r.Status),
            r.Likelihood, r.Impact, r.RiskScore,
            r.DepartmentNameAr, r.OwnerNameAr,
            r.CreatedAt.ToString("yyyy-MM-dd"),
        });

        return ExcelExporter.Build("سجل المخاطر", headers, rows);
    }

    // ── GetById ───────────────────────────────────────────────────────────────
    public async Task<Result<RiskDetailDto>> GetByIdAsync(int id)
    {
        var r = await _db.Risks.Include(x => x.Department).Include(x => x.Owner)
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (r is null) return Result<RiskDetailDto>.Failure("المخاطرة غير موجودة.");
        var dto = MapDetail(r);
        dto.LinkedTasksCount = await _db.Tasks.CountAsync(t => t.RiskId == id && !t.IsDeleted);
        return Result<RiskDetailDto>.Success(dto);
    }

    // ── Save ──────────────────────────────────────────────────────────────────
    public async Task<Result<RiskDetailDto>> SaveAsync(SaveRiskRequest req, string by)
    {
        if (await _db.Risks.AnyAsync(r => r.Code == req.Code && r.Id != req.Id && !r.IsDeleted))
            return Result<RiskDetailDto>.Failure("الرمز مستخدم مسبقاً.");

        Risk risk;
        if (req.Id == 0)
        {
            risk = new Risk { CreatedAt = DateTime.UtcNow, CreatedBy = by };
            _db.Risks.Add(risk);
        }
        else
        {
            risk = await _db.Risks.FirstOrDefaultAsync(r => r.Id == req.Id && !r.IsDeleted)
                   ?? throw new KeyNotFoundException();
            risk.ModifiedAt = DateTime.UtcNow; risk.ModifiedBy = by;
        }

        risk.TitleAr = req.TitleAr; risk.TitleEn = req.TitleEn; risk.Code = req.Code;
        risk.DescriptionAr = req.DescriptionAr; risk.MitigationPlanAr = req.MitigationPlanAr;
        risk.Category = req.Category; risk.Status = req.Status;
        risk.Likelihood = req.Likelihood; risk.Impact = req.Impact;
        risk.DepartmentId = req.DepartmentId; risk.OwnerId = req.OwnerId;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(risk.Id);
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    public async Task<Result<bool>> DeleteAsync(int id, string by)
    {
        var r = await _db.Risks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (r is null) return Result<bool>.Failure("المخاطرة غير موجودة.");
        r.IsDeleted = true; r.ModifiedAt = DateTime.UtcNow; r.ModifiedBy = by;
        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Heat Map ──────────────────────────────────────────────────────────────
    public async Task<List<RiskHeatMapItemDto>> GetHeatMapAsync() =>
        await _db.Risks
            .Include(r => r.Owner)
            .Where(r => !r.IsDeleted)
            .AsNoTracking()
            .Select(r => new RiskHeatMapItemDto
            {
                Id          = r.Id,
                TitleAr     = r.TitleAr,
                Code        = r.Code,
                Likelihood  = r.Likelihood,
                Impact      = r.Impact,
                RiskScore   = r.Likelihood * r.Impact,
                Status      = r.Status,
                OwnerNameAr = r.Owner != null ? r.Owner.FullNameAr : null,
            })
            .OrderByDescending(r => r.RiskScore)
            .ToListAsync();

    // ── KPI Impact Links ─────────────────────────────────────────────────────
    public async Task<List<RiskKpiLinkDto>> GetKpiLinksAsync(int riskId) =>
        await _db.RiskKpiMappings
            .Include(m => m.Kpi)
            .Where(m => m.RiskId == riskId)
            .AsNoTracking()
            .Select(m => new RiskKpiLinkDto
            {
                MappingId  = m.Id,
                KpiId      = m.KpiId,
                KpiCode    = m.Kpi.Code,
                KpiTitleAr = m.Kpi.TitleAr,
                Notes      = m.Notes,
            })
            .ToListAsync();

    public async Task<RiskKpiLinkDto> AddKpiLinkAsync(int riskId, int kpiId, string? notes)
    {
        if (await _db.RiskKpiMappings.AnyAsync(m => m.RiskId == riskId && m.KpiId == kpiId))
            throw new InvalidOperationException("الربط موجود مسبقاً.");

        var mapping = new RiskKpiMapping { RiskId = riskId, KpiId = kpiId, Notes = notes };
        _db.RiskKpiMappings.Add(mapping);
        await _db.SaveChangesAsync();

        var kpi = await _db.Kpis.AsNoTracking().FirstAsync(k => k.Id == kpiId);
        return new RiskKpiLinkDto
        {
            MappingId  = mapping.Id,
            KpiId      = kpi.Id,
            KpiCode    = kpi.Code,
            KpiTitleAr = kpi.TitleAr,
            Notes      = mapping.Notes,
        };
    }

    public async Task RemoveKpiLinkAsync(int mappingId)
    {
        var m = await _db.RiskKpiMappings.FindAsync(mappingId);
        if (m is not null) { _db.RiskKpiMappings.Remove(m); await _db.SaveChangesAsync(); }
    }

    // ── Projections ───────────────────────────────────────────────────────────
    private static readonly System.Linq.Expressions.Expression<Func<Risk, RiskListDto>> ToListDto = r =>
        new RiskListDto
        {
            Id = r.Id, TitleAr = r.TitleAr, TitleEn = r.TitleEn, Code = r.Code,
            Category = r.Category, Status = r.Status,
            Likelihood = r.Likelihood, Impact = r.Impact,
            RiskScore  = r.Likelihood * r.Impact,
            DepartmentNameAr = r.Department != null ? r.Department.NameAr : null,
            OwnerNameAr      = r.Owner      != null ? r.Owner.FullNameAr  : null,
            CreatedAt = r.CreatedAt,
        };

    private static RiskDetailDto MapDetail(Risk r) => new()
    {
        Id = r.Id, TitleAr = r.TitleAr, TitleEn = r.TitleEn, Code = r.Code,
        DescriptionAr = r.DescriptionAr, MitigationPlanAr = r.MitigationPlanAr,
        Category = r.Category, Status = r.Status,
        Likelihood = r.Likelihood, Impact = r.Impact, RiskScore = r.Likelihood * r.Impact,
        DepartmentId = r.DepartmentId, DepartmentNameAr = r.Department?.NameAr,
        OwnerId = r.OwnerId, OwnerNameAr = r.Owner?.FullNameAr,
        CreatedAt = r.CreatedAt,
    };
}
