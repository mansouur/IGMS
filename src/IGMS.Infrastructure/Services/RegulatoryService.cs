using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class RegulatoryService : IRegulatoryService
{
    private readonly TenantDbContext _db;
    public RegulatoryService(TenantDbContext db) => _db = db;

    // ── Frameworks ────────────────────────────────────────────────────────────

    public async Task<List<RegulatoryFrameworkDto>> GetFrameworksAsync()
    {
        var frameworks = await _db.RegulatoryFrameworks
            .AsNoTracking()
            .Where(f => f.IsActive && !f.IsDeleted)
            .OrderBy(f => f.Code)
            .ToListAsync();

        var result = new List<RegulatoryFrameworkDto>();
        foreach (var f in frameworks)
        {
            var controlIds = await _db.RegulatoryControls
                .AsNoTracking()
                .Where(c => c.RegulatoryFrameworkId == f.Id && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync();

            var mappedCount = await _db.ControlMappings
                .AsNoTracking()
                .Where(m => controlIds.Contains(m.RegulatoryControlId) && !m.IsDeleted)
                .Select(m => m.RegulatoryControlId)
                .Distinct()
                .CountAsync();

            result.Add(new RegulatoryFrameworkDto
            {
                Id           = f.Id,
                Code         = f.Code,
                NameAr       = f.NameAr,
                NameEn       = f.NameEn,
                Version      = f.Version,
                DescriptionAr = f.DescriptionAr,
                IsActive     = f.IsActive,
                ControlCount = controlIds.Count,
                MappedCount  = mappedCount,
            });
        }
        return result;
    }

    // ── Controls ──────────────────────────────────────────────────────────────

    public async Task<List<RegulatoryControlDto>> GetControlsByFrameworkAsync(int frameworkId, string? domain = null)
    {
        var q = _db.RegulatoryControls
            .AsNoTracking()
            .Include(c => c.Mappings.Where(m => !m.IsDeleted))
            .Where(c => c.RegulatoryFrameworkId == frameworkId && !c.IsDeleted);

        if (!string.IsNullOrEmpty(domain))
            q = q.Where(c => c.DomainAr == domain || c.DomainEn == domain);

        var controls = await q
            .OrderBy(c => c.ControlCode)
            .ToListAsync();

        // Enrich entity titles in one pass
        var policyIds      = controls.SelectMany(c => c.Mappings.Where(m => m.EntityType == "Policy")).Select(m => m.EntityId).Distinct().ToList();
        var riskIds        = controls.SelectMany(c => c.Mappings.Where(m => m.EntityType == "Risk")).Select(m => m.EntityId).Distinct().ToList();
        var controlTestIds = controls.SelectMany(c => c.Mappings.Where(m => m.EntityType == "ControlTest")).Select(m => m.EntityId).Distinct().ToList();

        var policyTitles      = await _db.Policies.AsNoTracking().Where(p => policyIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.TitleAr);
        var riskTitles        = await _db.Risks.AsNoTracking().Where(r => riskIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r.TitleAr);
        var controlTestTitles = await _db.ControlTests.AsNoTracking().Where(c => controlTestIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.TitleAr);

        return controls.Select(c => new RegulatoryControlDto
        {
            Id          = c.Id,
            ControlCode = c.ControlCode,
            DomainAr    = c.DomainAr,
            DomainEn    = c.DomainEn,
            TitleAr     = c.TitleAr,
            TitleEn     = c.TitleEn,
            DescriptionAr = c.DescriptionAr,
            Mappings    = c.Mappings.Select(m => new ControlMappingDto
            {
                Id                  = m.Id,
                RegulatoryControlId = m.RegulatoryControlId,
                ControlCode         = c.ControlCode,
                ControlTitleAr      = c.TitleAr,
                EntityType          = m.EntityType,
                EntityId            = m.EntityId,
                EntityTitle         = GetEntityTitle(m.EntityType, m.EntityId, policyTitles, riskTitles, controlTestTitles),
                ComplianceStatus    = m.ComplianceStatus.ToString(),
                Notes               = m.Notes,
            }).ToList(),
        }).ToList();
    }

    // ── Mappings ──────────────────────────────────────────────────────────────

    public async Task<List<ControlMappingDto>> GetMappingsForEntityAsync(string entityType, int entityId)
    {
        var mappings = await _db.ControlMappings
            .AsNoTracking()
            .Include(m => m.RegulatoryControl)
            .Where(m => m.EntityType == entityType && m.EntityId == entityId && !m.IsDeleted)
            .OrderBy(m => m.RegulatoryControl!.ControlCode)
            .ToListAsync();

        return mappings.Select(m => new ControlMappingDto
        {
            Id                  = m.Id,
            RegulatoryControlId = m.RegulatoryControlId,
            ControlCode         = m.RegulatoryControl?.ControlCode ?? "",
            ControlTitleAr      = m.RegulatoryControl?.TitleAr ?? "",
            EntityType          = m.EntityType,
            EntityId            = m.EntityId,
            EntityTitle         = "",
            ComplianceStatus    = m.ComplianceStatus.ToString(),
            Notes               = m.Notes,
        }).ToList();
    }

    public async Task<ControlMappingDto> CreateMappingAsync(SaveControlMappingRequest req)
    {
        if (!Enum.TryParse<ComplianceStatus>(req.ComplianceStatus, out var status))
            throw new InvalidOperationException("حالة الامتثال غير صالحة.");

        // Prevent duplicate mapping
        var exists = await _db.ControlMappings
            .AnyAsync(m => m.RegulatoryControlId == req.RegulatoryControlId
                           && m.EntityType == req.EntityType
                           && m.EntityId == req.EntityId
                           && !m.IsDeleted);
        if (exists)
            throw new InvalidOperationException("هذا الربط موجود بالفعل.");

        var mapping = new ControlMapping
        {
            RegulatoryControlId = req.RegulatoryControlId,
            EntityType          = req.EntityType,
            EntityId            = req.EntityId,
            ComplianceStatus    = status,
            Notes               = req.Notes,
            CreatedAt           = DateTime.UtcNow,
            CreatedBy           = "api",
        };

        _db.ControlMappings.Add(mapping);
        await _db.SaveChangesAsync();

        var control = await _db.RegulatoryControls.AsNoTracking()
            .FirstAsync(c => c.Id == req.RegulatoryControlId);

        return new ControlMappingDto
        {
            Id                  = mapping.Id,
            RegulatoryControlId = mapping.RegulatoryControlId,
            ControlCode         = control.ControlCode,
            ControlTitleAr      = control.TitleAr,
            EntityType          = mapping.EntityType,
            EntityId            = mapping.EntityId,
            EntityTitle         = "",
            ComplianceStatus    = mapping.ComplianceStatus.ToString(),
            Notes               = mapping.Notes,
        };
    }

    public async Task<ControlMappingDto> UpdateMappingAsync(int id, UpdateMappingStatusRequest req)
    {
        var mapping = await _db.ControlMappings
            .Include(m => m.RegulatoryControl)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted)
            ?? throw new InvalidOperationException("الربط غير موجود.");

        if (!Enum.TryParse<ComplianceStatus>(req.ComplianceStatus, out var status))
            throw new InvalidOperationException("حالة الامتثال غير صالحة.");

        mapping.ComplianceStatus = status;
        mapping.Notes            = req.Notes;
        mapping.ModifiedAt       = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new ControlMappingDto
        {
            Id                  = mapping.Id,
            RegulatoryControlId = mapping.RegulatoryControlId,
            ControlCode         = mapping.RegulatoryControl?.ControlCode ?? "",
            ControlTitleAr      = mapping.RegulatoryControl?.TitleAr ?? "",
            EntityType          = mapping.EntityType,
            EntityId            = mapping.EntityId,
            ComplianceStatus    = mapping.ComplianceStatus.ToString(),
            Notes               = mapping.Notes,
        };
    }

    public async Task DeleteMappingAsync(int id)
    {
        var mapping = await _db.ControlMappings.FindAsync(id)
            ?? throw new InvalidOperationException("الربط غير موجود.");
        mapping.IsDeleted = true;
        mapping.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Coverage ──────────────────────────────────────────────────────────────

    public async Task<ComplianceCoverageDto> GetCoverageAsync(int frameworkId)
    {
        var framework = await _db.RegulatoryFrameworks.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == frameworkId && !f.IsDeleted)
            ?? throw new InvalidOperationException("الإطار غير موجود.");

        var controls = await _db.RegulatoryControls
            .AsNoTracking()
            .Where(c => c.RegulatoryFrameworkId == frameworkId && !c.IsDeleted)
            .ToListAsync();

        var controlIds = controls.Select(c => c.Id).ToList();

        // Best mapping per control (highest compliance status wins)
        var mappings = await _db.ControlMappings
            .AsNoTracking()
            .Where(m => controlIds.Contains(m.RegulatoryControlId) && !m.IsDeleted)
            .GroupBy(m => m.RegulatoryControlId)
            .Select(g => new
            {
                ControlId = g.Key,
                BestStatus = g.Max(m => (int)m.ComplianceStatus),
            })
            .ToDictionaryAsync(x => x.ControlId, x => (ComplianceStatus)x.BestStatus);

        var domainGroups = controls.GroupBy(c => c.DomainAr);
        var domains = domainGroups.Select(g =>
        {
            var ids = g.Select(c => c.Id).ToList();
            return new CoverageDomainDto
            {
                DomainAr           = g.Key,
                TotalControls      = ids.Count,
                Compliant          = ids.Count(id => mappings.GetValueOrDefault(id) == ComplianceStatus.Compliant),
                PartiallyCompliant = ids.Count(id => mappings.GetValueOrDefault(id) == ComplianceStatus.PartiallyCompliant),
                NonCompliant       = ids.Count(id => mappings.GetValueOrDefault(id) == ComplianceStatus.NonCompliant),
                NotAssessed        = ids.Count(id => !mappings.ContainsKey(id) || mappings[id] == ComplianceStatus.NotAssessed),
            };
        }).ToList();

        var total      = controls.Count;
        var compliant  = domains.Sum(d => d.Compliant);
        var partial    = domains.Sum(d => d.PartiallyCompliant);
        var nonComp    = domains.Sum(d => d.NonCompliant);
        var notAssessed = domains.Sum(d => d.NotAssessed);

        return new ComplianceCoverageDto
        {
            FrameworkId       = frameworkId,
            FrameworkName     = framework.NameAr,
            TotalControls     = total,
            Compliant         = compliant,
            PartiallyCompliant = partial,
            NonCompliant      = nonComp,
            NotAssessed       = notAssessed,
            CoveragePercent   = total == 0 ? 0 : Math.Round((compliant + partial * 0.5) / total * 100, 1),
            Domains           = domains,
        };
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static string GetEntityTitle(
        string entityType, int entityId,
        Dictionary<int, string> policyTitles,
        Dictionary<int, string> riskTitles,
        Dictionary<int, string> controlTestTitles) =>
        entityType switch
        {
            "Policy"      => policyTitles.GetValueOrDefault(entityId, $"سياسة #{entityId}"),
            "Risk"        => riskTitles.GetValueOrDefault(entityId, $"مخاطرة #{entityId}"),
            "ControlTest" => controlTestTitles.GetValueOrDefault(entityId, $"ضابط #{entityId}"),
            _             => $"{entityType} #{entityId}",
        };
}
