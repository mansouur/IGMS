using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class KpiService : IKpiService
{
    private readonly TenantDbContext _db;
    public KpiService(TenantDbContext db) => _db = db;

    // ── Shared filter builder ─────────────────────────────────────────────────
    private IQueryable<Kpi> BuildQuery(KpiQuery q)
    {
        var query = _db.Kpis
            .Include(k => k.Department).Include(k => k.Owner)
            .Where(k => !k.IsDeleted).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(k => k.TitleAr.Contains(q.Search) || k.Code.Contains(q.Search));
        if (q.Status.HasValue) query = query.Where(k => k.Status == q.Status.Value);
        if (q.Year.HasValue)   query = query.Where(k => k.Year   == q.Year.Value);

        return query;
    }

    // ── Paged list ────────────────────────────────────────────────────────────
    public async Task<Result<PagedResult<KpiListDto>>> GetPagedAsync(KpiQuery q)
    {
        var query = BuildQuery(q);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(k => k.Year).ThenBy(k => k.Quarter)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(ToListDto)
            .ToListAsync();

        return Result<PagedResult<KpiListDto>>.Success(
            PagedResult<KpiListDto>.Create(items, total, q.Page, q.PageSize));
    }

    // ── Export (no pagination) ────────────────────────────────────────────────
    public async Task<byte[]> ExportAsync(KpiQuery q)
    {
        var items = await BuildQuery(q)
            .OrderByDescending(k => k.Year).ThenBy(k => k.Quarter)
            .Select(ToListDto)
            .ToListAsync();

        var headers = new[]
        {
            "الرمز", "المؤشر", "الوحدة", "المستهدف", "الفعلي",
            "نسبة الإنجاز", "السنة", "الربع", "الحالة",
            "القسم", "المسؤول",
        };

        string StatusLabel(KpiStatus s) => s switch
        {
            KpiStatus.OnTrack => "في المسار",
            KpiStatus.AtRisk  => "في خطر",
            KpiStatus.Behind  => "متأخر",
            _                 => s.ToString(),
        };

        var rows = items.Select(k =>
        {
            var pct = k.TargetValue == 0 ? 0 : Math.Round(k.ActualValue / k.TargetValue * 100, 1);
            return new object?[]
            {
                k.Code, k.TitleAr, k.Unit, k.TargetValue, k.ActualValue,
                $"{pct}%", k.Year,
                k.Quarter.HasValue ? $"ق{k.Quarter}" : "سنوي",
                StatusLabel(k.Status), k.DepartmentNameAr, k.OwnerNameAr,
            };
        });

        return ExcelExporter.Build("مؤشرات الأداء", headers, rows);
    }

    // ── GetById ───────────────────────────────────────────────────────────────
    public async Task<Result<KpiDetailDto>> GetByIdAsync(int id)
    {
        var k = await _db.Kpis.Include(x => x.Department).Include(x => x.Owner)
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (k is null) return Result<KpiDetailDto>.Failure("المؤشر غير موجود.");
        return Result<KpiDetailDto>.Success(MapDetail(k));
    }

    // ── Save ──────────────────────────────────────────────────────────────────
    public async Task<Result<KpiDetailDto>> SaveAsync(SaveKpiRequest req, string by)
    {
        if (await _db.Kpis.AnyAsync(k => k.Code == req.Code && k.Id != req.Id && !k.IsDeleted))
            return Result<KpiDetailDto>.Failure("الرمز مستخدم مسبقاً.");

        Kpi kpi;
        if (req.Id == 0)
        {
            kpi = new Kpi { CreatedAt = DateTime.UtcNow, CreatedBy = by };
            _db.Kpis.Add(kpi);
        }
        else
        {
            kpi = await _db.Kpis.FirstOrDefaultAsync(k => k.Id == req.Id && !k.IsDeleted)
                  ?? throw new KeyNotFoundException();
            kpi.ModifiedAt = DateTime.UtcNow; kpi.ModifiedBy = by;
        }

        kpi.TitleAr = req.TitleAr; kpi.TitleEn = req.TitleEn; kpi.Code = req.Code;
        kpi.Unit = req.Unit; kpi.TargetValue = req.TargetValue; kpi.ActualValue = req.ActualValue;
        kpi.Year = req.Year; kpi.Quarter = req.Quarter; kpi.Status = req.Status;
        kpi.DepartmentId = req.DepartmentId; kpi.OwnerId = req.OwnerId;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(kpi.Id);
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    public async Task<Result<bool>> DeleteAsync(int id, string by)
    {
        var k = await _db.Kpis.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (k is null) return Result<bool>.Failure("المؤشر غير موجود.");
        k.IsDeleted = true; k.ModifiedAt = DateTime.UtcNow; k.ModifiedBy = by;
        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Risk Links ───────────────────────────────────────────────────────────
    public async Task<List<KpiRiskLinkDto>> GetRiskLinksAsync(int kpiId) =>
        await _db.RiskKpiMappings
            .Include(m => m.Risk)
            .Where(m => m.KpiId == kpiId && !m.Risk.IsDeleted)
            .AsNoTracking()
            .Select(m => new KpiRiskLinkDto
            {
                MappingId   = m.Id,
                RiskId      = m.RiskId,
                RiskCode    = m.Risk.Code,
                RiskTitleAr = m.Risk.TitleAr,
                RiskScore   = m.Risk.Likelihood * m.Risk.Impact,
                RiskStatus  = m.Risk.Status,
                Notes       = m.Notes,
            })
            .ToListAsync();

    // ── Projections ───────────────────────────────────────────────────────────
    private static readonly System.Linq.Expressions.Expression<Func<Kpi, KpiListDto>> ToListDto = k =>
        new KpiListDto
        {
            Id = k.Id, TitleAr = k.TitleAr, TitleEn = k.TitleEn, Code = k.Code,
            Unit = k.Unit, TargetValue = k.TargetValue, ActualValue = k.ActualValue,
            Year = k.Year, Quarter = k.Quarter, Status = k.Status,
            DepartmentNameAr = k.Department != null ? k.Department.NameAr : null,
            OwnerNameAr      = k.Owner      != null ? k.Owner.FullNameAr  : null,
            CreatedAt = k.CreatedAt,
        };

    private static KpiDetailDto MapDetail(Kpi k) => new()
    {
        Id = k.Id, TitleAr = k.TitleAr, TitleEn = k.TitleEn, Code = k.Code,
        Unit = k.Unit, TargetValue = k.TargetValue, ActualValue = k.ActualValue,
        Year = k.Year, Quarter = k.Quarter, Status = k.Status,
        DepartmentId = k.DepartmentId, DepartmentNameAr = k.Department?.NameAr,
        OwnerId = k.OwnerId, OwnerNameAr = k.Owner?.FullNameAr,
        CreatedAt = k.CreatedAt,
    };
}
