using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class ControlTestService : IControlTestService
{
    private readonly TenantDbContext _db;
    private readonly string          _uploadsRoot;

    private static readonly HashSet<string> _allowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "image/png", "image/jpeg", "image/gif",
    };

    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

    public ControlTestService(TenantDbContext db, IWebHostEnvironment env)
    {
        _db          = db;
        _uploadsRoot = Path.Combine(env.ContentRootPath, "uploads");
    }

    // ── Paged list ────────────────────────────────────────────────────────────

    public async Task<Result<PagedResult<ControlTestListDto>>> GetPagedAsync(ControlTestQuery q)
    {
        var query = _db.ControlTests
            .Include(t => t.TestedBy)
            .Where(t => !t.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(t => t.TitleAr.Contains(q.Search) || t.Code.Contains(q.Search));

        if (!string.IsNullOrWhiteSpace(q.EntityType))
            query = query.Where(t => t.EntityType == q.EntityType);

        if (q.EntityId.HasValue)
            query = query.Where(t => t.EntityId == q.EntityId.Value);

        if (q.Effectiveness.HasValue)
            query = query.Where(t => t.Effectiveness == q.Effectiveness.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(t => new ControlTestListDto
            {
                Id            = t.Id,
                TitleAr       = t.TitleAr,
                TitleEn       = t.TitleEn,
                Code          = t.Code,
                EntityType    = t.EntityType,
                EntityId      = t.EntityId,
                Effectiveness = t.Effectiveness,
                TestedAt      = t.TestedAt,
                NextTestDate  = t.NextTestDate,
                TestedByName  = t.TestedBy != null ? t.TestedBy.FullNameAr : null,
                EvidenceCount = t.Evidences.Count(e => !e.IsDeleted),
                CreatedAt     = t.CreatedAt,
            })
            .ToListAsync();

        // Enrich EntityTitleAr
        await EnrichEntityTitlesAsync(items);

        return Result<PagedResult<ControlTestListDto>>.Success(
            PagedResult<ControlTestListDto>.Create(items, total, q.Page, q.PageSize));
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    public async Task<Result<ControlTestDetailDto>> GetByIdAsync(int id)
    {
        var t = await _db.ControlTests
            .Include(x => x.TestedBy)
            .Include(x => x.Evidences.Where(e => !e.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (t is null)
            return Result<ControlTestDetailDto>.Failure("اختبار الضابط غير موجود.");

        string? entityTitle = await GetEntityTitleAsync(t.EntityType, t.EntityId);

        var dto = new ControlTestDetailDto
        {
            Id            = t.Id,
            TitleAr       = t.TitleAr,
            TitleEn       = t.TitleEn,
            Code          = t.Code,
            DescriptionAr = t.DescriptionAr,
            EntityType    = t.EntityType,
            EntityId      = t.EntityId,
            EntityTitleAr = entityTitle,
            Effectiveness = t.Effectiveness,
            TestedAt      = t.TestedAt,
            NextTestDate  = t.NextTestDate,
            TestedById    = t.TestedById,
            TestedByName  = t.TestedBy?.FullNameAr,
            FindingsAr    = t.FindingsAr,
            EvidenceCount = t.Evidences.Count,
            CreatedAt     = t.CreatedAt,
            Evidences     = t.Evidences.Select(e => new ControlEvidenceDto
            {
                Id            = e.Id,
                FileName      = e.FileName,
                ContentType   = e.ContentType,
                FileSizeBytes = e.FileSizeBytes,
                UploadedBy    = e.UploadedBy,
                UploadedAt    = e.UploadedAt,
            }).ToList(),
        };

        return Result<ControlTestDetailDto>.Success(dto);
    }

    // ── Save (create / update) ────────────────────────────────────────────────

    public async Task<Result<ControlTestDetailDto>> SaveAsync(SaveControlTestRequest req, string by)
    {
        if (req.EntityType != "Policy" && req.EntityType != "Risk")
            return Result<ControlTestDetailDto>.Failure("EntityType يجب أن يكون 'Policy' أو 'Risk'.");

        ControlTest entity;

        if (req.Id == 0)
        {
            entity = new ControlTest { CreatedAt = DateTime.UtcNow, CreatedBy = by };
            _db.ControlTests.Add(entity);
        }
        else
        {
            entity = await _db.ControlTests.FirstOrDefaultAsync(t => t.Id == req.Id && !t.IsDeleted)
                     ?? throw new KeyNotFoundException($"ControlTest {req.Id} not found.");
            entity.ModifiedAt = DateTime.UtcNow;
            entity.ModifiedBy = by;
        }

        entity.TitleAr       = req.TitleAr;
        entity.TitleEn       = req.TitleEn;
        entity.Code          = req.Code;
        entity.DescriptionAr = req.DescriptionAr;
        entity.EntityType    = req.EntityType;
        entity.EntityId      = req.EntityId;
        entity.TestedById    = req.TestedById;
        entity.TestedAt      = req.TestedAt;
        entity.NextTestDate  = req.NextTestDate;
        entity.Effectiveness = req.Effectiveness;
        entity.FindingsAr    = req.FindingsAr;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task<Result<bool>> DeleteAsync(int id, string by)
    {
        var entity = await _db.ControlTests.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (entity is null)
            return Result<bool>.Failure("اختبار الضابط غير موجود.");

        entity.IsDeleted  = true;
        entity.DeletedAt  = DateTime.UtcNow;
        entity.DeletedBy  = by;
        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Evidence Upload ───────────────────────────────────────────────────────

    public async Task<Result<ControlEvidenceDto>> UploadEvidenceAsync(
        int controlTestId, Stream stream, string fileName,
        string contentType, long fileSize, string tenantKey, string uploadedBy)
    {
        if (fileSize == 0)
            return Result<ControlEvidenceDto>.Failure("الملف فارغ.");

        if (fileSize > MaxFileSizeBytes)
            return Result<ControlEvidenceDto>.Failure("حجم الملف يتجاوز 20 ميغابايت.");

        if (!_allowedTypes.Contains(contentType))
            return Result<ControlEvidenceDto>.Failure("نوع الملف غير مسموح به.");

        var testExists = await _db.ControlTests.AnyAsync(t => t.Id == controlTestId && !t.IsDeleted);
        if (!testExists)
            return Result<ControlEvidenceDto>.Failure("اختبار الضابط غير موجود.");

        var safeFileName = Path.GetFileName(fileName);
        var storedName   = $"{Guid.NewGuid():N}_{safeFileName}";
        var dir          = Path.Combine(_uploadsRoot, tenantKey, "control-tests", controlTestId.ToString());
        Directory.CreateDirectory(dir);

        var fullPath   = Path.Combine(dir, storedName);
        var storedPath = Path.Combine(tenantKey, "control-tests", controlTestId.ToString(), storedName);

        await using (var fs = File.Create(fullPath))
            await stream.CopyToAsync(fs);

        var evidence = new ControlEvidence
        {
            ControlTestId = controlTestId,
            FileName      = fileName,
            StoredPath    = storedPath,
            ContentType   = contentType,
            FileSizeBytes = fileSize,
            UploadedBy    = uploadedBy,
            UploadedAt    = DateTime.UtcNow,
            CreatedAt     = DateTime.UtcNow,
            CreatedBy     = uploadedBy,
        };

        _db.ControlEvidences.Add(evidence);
        await _db.SaveChangesAsync();

        return Result<ControlEvidenceDto>.Success(new ControlEvidenceDto
        {
            Id            = evidence.Id,
            FileName      = evidence.FileName,
            ContentType   = evidence.ContentType,
            FileSizeBytes = evidence.FileSizeBytes,
            UploadedBy    = evidence.UploadedBy,
            UploadedAt    = evidence.UploadedAt,
        });
    }

    // ── Evidence Download ─────────────────────────────────────────────────────

    public async Task<Result<DownloadResult>> DownloadEvidenceAsync(int evidenceId)
    {
        var ev = await _db.ControlEvidences.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == evidenceId && !e.IsDeleted);

        if (ev is null)
            return Result<DownloadResult>.Failure("الدليل غير موجود.");

        var fullPath = Path.Combine(_uploadsRoot, ev.StoredPath);
        if (!File.Exists(fullPath))
            return Result<DownloadResult>.Failure("الملف غير موجود على الخادم.");

        var data = await File.ReadAllBytesAsync(fullPath);
        return Result<DownloadResult>.Success(new DownloadResult
        {
            Data        = data,
            ContentType = ev.ContentType,
            FileName    = ev.FileName,
        });
    }

    // ── Evidence Delete ───────────────────────────────────────────────────────

    public async Task<Result<bool>> DeleteEvidenceAsync(int evidenceId)
    {
        var ev = await _db.ControlEvidences.FirstOrDefaultAsync(e => e.Id == evidenceId && !e.IsDeleted);
        if (ev is null)
            return Result<bool>.Failure("الدليل غير موجود.");

        var fullPath = Path.Combine(_uploadsRoot, ev.StoredPath);
        if (File.Exists(fullPath)) File.Delete(fullPath);

        ev.IsDeleted = true;
        ev.DeletedAt = DateTime.UtcNow;
        ev.DeletedBy = ev.UploadedBy;
        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task EnrichEntityTitlesAsync(List<ControlTestListDto> items)
    {
        var policyIds = items.Where(i => i.EntityType == "Policy").Select(i => i.EntityId).Distinct().ToList();
        var riskIds   = items.Where(i => i.EntityType == "Risk").Select(i => i.EntityId).Distinct().ToList();

        var policies = policyIds.Any()
            ? await _db.Policies.AsNoTracking()
                .Where(p => policyIds.Contains(p.Id))
                .Select(p => new { p.Id, p.TitleAr })
                .ToDictionaryAsync(p => p.Id, p => p.TitleAr)
            : new Dictionary<int, string>();

        var risks = riskIds.Any()
            ? await _db.Risks.AsNoTracking()
                .Where(r => riskIds.Contains(r.Id))
                .Select(r => new { r.Id, r.TitleAr })
                .ToDictionaryAsync(r => r.Id, r => r.TitleAr)
            : new Dictionary<int, string>();

        foreach (var item in items)
        {
            item.EntityTitleAr = item.EntityType == "Policy"
                ? policies.GetValueOrDefault(item.EntityId)
                : risks.GetValueOrDefault(item.EntityId);
        }
    }

    private async Task<string?> GetEntityTitleAsync(string entityType, int entityId)
    {
        if (entityType == "Policy")
            return await _db.Policies.AsNoTracking()
                .Where(p => p.Id == entityId)
                .Select(p => (string?)p.TitleAr)
                .FirstOrDefaultAsync();

        if (entityType == "Risk")
            return await _db.Risks.AsNoTracking()
                .Where(r => r.Id == entityId)
                .Select(r => (string?)r.TitleAr)
                .FirstOrDefaultAsync();

        return null;
    }
}
