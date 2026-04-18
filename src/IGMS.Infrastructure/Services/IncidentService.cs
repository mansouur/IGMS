using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class IncidentService : IIncidentService
{
    private readonly TenantDbContext _db;
    public IncidentService(TenantDbContext db) => _db = db;

    // ── List ──────────────────────────────────────────────────────────────────

    public async Task<List<IncidentListDto>> GetListAsync(string? status, string? severity, int? departmentId, int? riskId = null)
    {
        var q = _db.Incidents
            .AsNoTracking()
            .Include(i => i.Department)
            .Include(i => i.ReportedBy)
            .Include(i => i.Risk)
            .Where(i => !i.IsDeleted);

        if (!string.IsNullOrEmpty(status) &&
            Enum.TryParse<IncidentStatus>(status, out var st))
            q = q.Where(i => i.Status == st);

        if (!string.IsNullOrEmpty(severity) &&
            Enum.TryParse<IncidentSeverity>(severity, out var sv))
            q = q.Where(i => i.Severity == sv);

        if (departmentId.HasValue)
            q = q.Where(i => i.DepartmentId == departmentId);

        if (riskId.HasValue)
            q = q.Where(i => i.RiskId == riskId);

        var list = await q.OrderByDescending(i => i.OccurredAt).ToListAsync();
        return list.Select(MapList).ToList();
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    public async Task<IncidentDetailDto?> GetByIdAsync(int id)
    {
        var i = await LoadFull(id);
        return i == null ? null : MapDetail(i);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<IncidentDetailDto> CreateAsync(SaveIncidentRequest req, int reportedById)
    {
        if (!Enum.TryParse<IncidentSeverity>(req.Severity, out var sev))
            throw new InvalidOperationException($"مستوى خطورة غير صالح: {req.Severity}");
        if (!Enum.TryParse<IncidentStatus>(req.Status, out var st))
            throw new InvalidOperationException($"حالة غير صالحة: {req.Status}");

        var incident = new Incident
        {
            TitleAr        = req.TitleAr,
            TitleEn        = req.TitleEn,
            DescriptionAr  = req.DescriptionAr,
            Severity       = sev,
            Status         = st,
            OccurredAt     = req.OccurredAt,
            DepartmentId   = req.DepartmentId,
            ReportedById   = reportedById,
            RiskId         = req.RiskId,
            TaskId         = req.TaskId,
            ResolutionNotes = req.ResolutionNotes,
            CreatedAt      = DateTime.UtcNow,
            CreatedBy      = "api",
        };

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync();

        var loaded = await LoadFull(incident.Id);
        return MapDetail(loaded!);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<IncidentDetailDto> UpdateAsync(int id, SaveIncidentRequest req)
    {
        var incident = await _db.Incidents.FindAsync(id)
            ?? throw new InvalidOperationException("الحادثة غير موجودة.");

        if (!Enum.TryParse<IncidentSeverity>(req.Severity, out var sev))
            throw new InvalidOperationException($"مستوى خطورة غير صالح: {req.Severity}");
        if (!Enum.TryParse<IncidentStatus>(req.Status, out var st))
            throw new InvalidOperationException($"حالة غير صالحة: {req.Status}");

        incident.TitleAr        = req.TitleAr;
        incident.TitleEn        = req.TitleEn;
        incident.DescriptionAr  = req.DescriptionAr;
        incident.Severity       = sev;
        incident.Status         = st;
        incident.OccurredAt     = req.OccurredAt;
        incident.DepartmentId   = req.DepartmentId;
        incident.RiskId         = req.RiskId;
        incident.TaskId         = req.TaskId;
        incident.ResolutionNotes = req.ResolutionNotes;
        incident.ModifiedAt     = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var loaded = await LoadFull(incident.Id);
        return MapDetail(loaded!);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task DeleteAsync(int id)
    {
        var incident = await _db.Incidents.FindAsync(id)
            ?? throw new InvalidOperationException("الحادثة غير موجودة.");
        incident.IsDeleted = true;
        incident.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Resolve ───────────────────────────────────────────────────────────────

    public async Task<IncidentDetailDto> ResolveAsync(int id, string? resolutionNotes)
    {
        var incident = await _db.Incidents.FindAsync(id)
            ?? throw new InvalidOperationException("الحادثة غير موجودة.");

        incident.Status          = IncidentStatus.Resolved;
        incident.ResolutionNotes = resolutionNotes;
        incident.ResolvedAt      = DateTime.UtcNow;
        incident.ModifiedAt      = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var loaded = await LoadFull(incident.Id);
        return MapDetail(loaded!);
    }

    // ── Export ────────────────────────────────────────────────────────────────

    public async Task<byte[]> ExportAsync(string? status, string? severity)
    {
        var list = await GetListAsync(status, severity, null);

        var headers = new[] { "#", "العنوان", "الخطورة", "الحالة", "تاريخ الوقوع", "القسم", "المُبلِّغ", "المخاطرة المرتبطة" };
        var rows = list.Select((inc, i) => new object?[]
        {
            i + 1,
            inc.TitleAr,
            inc.Severity,
            inc.Status,
            inc.OccurredAt,
            inc.DepartmentName,
            inc.ReportedByName,
            inc.RiskTitleAr,
        });

        return ExcelExporter.Build("الحوادث", headers, rows);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Incident?> LoadFull(int id) =>
        await _db.Incidents
            .AsNoTracking()
            .Include(i => i.Department)
            .Include(i => i.ReportedBy)
            .Include(i => i.Risk)
            .Include(i => i.Task)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

    private static IncidentListDto MapList(Incident i) => new()
    {
        Id             = i.Id,
        TitleAr        = i.TitleAr,
        TitleEn        = i.TitleEn,
        Severity       = i.Severity.ToString(),
        Status         = i.Status.ToString(),
        OccurredAt     = i.OccurredAt,
        DepartmentName = i.Department?.NameAr,
        ReportedByName = i.ReportedBy?.FullNameAr,
        RiskId         = i.RiskId,
        RiskTitleAr    = i.Risk?.TitleAr,
    };

    private static IncidentDetailDto MapDetail(Incident i) => new()
    {
        Id              = i.Id,
        TitleAr         = i.TitleAr,
        TitleEn         = i.TitleEn,
        DescriptionAr   = i.DescriptionAr,
        Severity        = i.Severity.ToString(),
        Status          = i.Status.ToString(),
        OccurredAt      = i.OccurredAt,
        DepartmentId    = i.DepartmentId,
        DepartmentName  = i.Department?.NameAr,
        ReportedById    = i.ReportedById,
        ReportedByName  = i.ReportedBy?.FullNameAr,
        RiskId          = i.RiskId,
        RiskTitleAr     = i.Risk?.TitleAr,
        TaskId          = i.TaskId,
        TaskTitleAr     = i.Task?.TitleAr,
        ResolutionNotes = i.ResolutionNotes,
        ResolvedAt      = i.ResolvedAt,
        CreatedAt       = i.CreatedAt,
    };
}
