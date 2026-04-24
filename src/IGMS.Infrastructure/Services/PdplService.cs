using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class PdplService : IPdplService
{
    private readonly TenantDbContext _db;
    public PdplService(TenantDbContext db) => _db = db;

    // ── Records ───────────────────────────────────────────────────────────────

    public async Task<PagedResult<PdplRecordListDto>> GetPagedAsync(PdplQuery query)
    {
        var q = _db.PdplRecords
            .AsNoTracking()
            .Include(r => r.Department)
            .Include(r => r.Owner)
            .Include(r => r.Consents.Where(c => !c.IsDeleted))
            .Include(r => r.DataRequests.Where(d => !d.IsDeleted))
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(r => r.TitleAr.Contains(query.Search) ||
                              (r.TitleEn != null && r.TitleEn.Contains(query.Search)));

        if (!string.IsNullOrEmpty(query.Status) &&
            Enum.TryParse<PdplRecordStatus>(query.Status, out var st))
            q = q.Where(r => r.Status == st);

        if (!string.IsNullOrEmpty(query.DataCategory) &&
            Enum.TryParse<DataCategory>(query.DataCategory, out var dc))
            q = q.Where(r => r.DataCategory == dc);

        if (query.DepartmentId.HasValue)
            q = q.Where(r => r.DepartmentId == query.DepartmentId.Value);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<PdplRecordListDto>
        {
            Items       = items.Select(MapList).ToList(),
            TotalCount  = total,
            CurrentPage = query.Page,
            PageSize    = query.PageSize,
        };
    }

    public async Task<PdplRecordDetailDto?> GetByIdAsync(int id)
    {
        var r = await LoadFull(id);
        return r == null ? null : MapDetail(r);
    }

    public async Task<PdplRecordDetailDto> CreateAsync(SavePdplRecordRequest req, int createdById)
    {
        var record = Apply(new PdplRecord
        {
            CreatedBy = createdById.ToString(),
            CreatedAt = DateTime.UtcNow,
        }, req);

        _db.PdplRecords.Add(record);
        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(record.Id))!);
    }

    public async Task<PdplRecordDetailDto> UpdateAsync(int id, SavePdplRecordRequest req)
    {
        var record = await _db.PdplRecords.FindAsync(id)
            ?? throw new InvalidOperationException("السجل غير موجود.");

        Apply(record, req);
        record.ModifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(id))!);
    }

    public async Task DeleteAsync(int id)
    {
        var r = await _db.PdplRecords.FindAsync(id)
            ?? throw new InvalidOperationException("السجل غير موجود.");
        r.IsDeleted = true; r.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PdplRecordDetailDto> MarkReviewedAsync(int id)
    {
        var r = await _db.PdplRecords.FindAsync(id)
            ?? throw new InvalidOperationException("السجل غير موجود.");
        r.LastReviewedAt = DateTime.UtcNow;
        r.ModifiedAt     = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(id))!);
    }

    // ── Consents ──────────────────────────────────────────────────────────────

    public async Task<PdplConsentDto> AddConsentAsync(int recordId, SaveConsentRequest req)
    {
        _ = await _db.PdplRecords.FindAsync(recordId)
            ?? throw new InvalidOperationException("السجل غير موجود.");

        var consent = new PdplConsent
        {
            PdplRecordId   = recordId,
            SubjectNameAr  = req.SubjectNameAr,
            SubjectEmail   = req.SubjectEmail,
            SubjectIdNumber= req.SubjectIdNumber,
            IsConsented    = req.IsConsented,
            Notes          = req.Notes,
            ConsentedAt    = DateTime.UtcNow,
            CreatedAt      = DateTime.UtcNow,
        };

        _db.PdplConsents.Add(consent);
        await _db.SaveChangesAsync();
        return MapConsent(consent);
    }

    public async Task<PdplConsentDto> WithdrawConsentAsync(int recordId, int consentId)
    {
        var consent = await _db.PdplConsents
            .FirstOrDefaultAsync(c => c.Id == consentId && c.PdplRecordId == recordId)
            ?? throw new InvalidOperationException("الموافقة غير موجودة.");

        consent.IsConsented  = false;
        consent.WithdrawnAt  = DateTime.UtcNow;
        consent.ModifiedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapConsent(consent);
    }

    // ── Data Requests ─────────────────────────────────────────────────────────

    public async Task<PagedResult<PdplDataRequestDto>> GetRequestsAsync(PdplRequestQuery query)
    {
        var q = _db.PdplDataRequests
            .AsNoTracking()
            .Include(d => d.AssignedTo)
            .Where(d => !d.IsDeleted);

        if (!string.IsNullOrEmpty(query.Status) &&
            Enum.TryParse<DataRequestStatus>(query.Status, out var st))
            q = q.Where(d => d.Status == st);

        if (!string.IsNullOrEmpty(query.RequestType) &&
            Enum.TryParse<DataRequestType>(query.RequestType, out var rt))
            q = q.Where(d => d.RequestType == rt);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(d => d.ReceivedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<PdplDataRequestDto>
        {
            Items       = items.Select(MapRequest).ToList(),
            TotalCount  = total,
            CurrentPage = query.Page,
            PageSize    = query.PageSize,
        };
    }

    public async Task<PdplDataRequestDto> AddRequestAsync(int recordId, SaveDataRequestRequest req, int createdById)
    {
        _ = await _db.PdplRecords.FindAsync(recordId)
            ?? throw new InvalidOperationException("السجل غير موجود.");

        if (!Enum.TryParse<DataRequestType>(req.RequestType, out var rt))
            throw new InvalidOperationException("نوع الطلب غير صالح.");

        var dr = new PdplDataRequest
        {
            PdplRecordId  = recordId,
            RequestType   = rt,
            Status        = DataRequestStatus.Pending,
            SubjectNameAr = req.SubjectNameAr,
            SubjectEmail  = req.SubjectEmail,
            DetailsAr     = req.DetailsAr,
            AssignedToId  = req.AssignedToId,
            ReceivedAt    = DateTime.UtcNow,
            DueAt         = DateTime.UtcNow.AddDays(30),   // PDPL: 30 days
            CreatedAt     = DateTime.UtcNow,
            CreatedBy     = createdById.ToString(),
        };

        _db.PdplDataRequests.Add(dr);
        await _db.SaveChangesAsync();

        var full = await _db.PdplDataRequests.Include(d => d.AssignedTo).FirstAsync(d => d.Id == dr.Id);
        return MapRequest(full);
    }

    public async Task<PdplDataRequestDto> ResolveRequestAsync(int recordId, int requestId, ResolveDataRequestRequest req)
    {
        var dr = await _db.PdplDataRequests
            .Include(d => d.AssignedTo)
            .FirstOrDefaultAsync(d => d.Id == requestId && d.PdplRecordId == recordId)
            ?? throw new InvalidOperationException("الطلب غير موجود.");

        dr.Status       = req.Rejected ? DataRequestStatus.Rejected : DataRequestStatus.Completed;
        dr.ResolutionAr = req.ResolutionAr;
        dr.ResolvedAt   = DateTime.UtcNow;
        dr.ModifiedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapRequest(dr);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Task<PdplRecord?> LoadFull(int id) =>
        _db.PdplRecords
            .Include(r => r.Department)
            .Include(r => r.Owner)
            .Include(r => r.Consents.Where(c => !c.IsDeleted))
            .Include(r => r.DataRequests.Where(d => !d.IsDeleted))
                .ThenInclude(d => d.AssignedTo)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

    private static PdplRecord Apply(PdplRecord r, SavePdplRecordRequest req)
    {
        if (!Enum.TryParse<DataCategory>(req.DataCategory, out var dc)) dc = DataCategory.Basic;
        if (!Enum.TryParse<LegalBasis>(req.LegalBasis, out var lb)) lb = LegalBasis.LegalObligation;
        if (!Enum.TryParse<PdplRecordStatus>(req.Status, out var st)) st = PdplRecordStatus.Active;

        r.TitleAr               = req.TitleAr;
        r.TitleEn               = req.TitleEn;
        r.PurposeAr             = req.PurposeAr;
        r.DataSubjectsAr        = req.DataSubjectsAr;
        r.RetentionPeriod       = req.RetentionPeriod;
        r.SecurityMeasures      = req.SecurityMeasures;
        r.DataCategory          = dc;
        r.LegalBasis            = lb;
        r.Status                = st;
        r.IsThirdPartySharing   = req.IsThirdPartySharing;
        r.ThirdPartyDetails     = req.ThirdPartyDetails;
        r.IsCrossBorderTransfer = req.IsCrossBorderTransfer;
        r.TransferCountry       = req.TransferCountry;
        r.TransferSafeguards    = req.TransferSafeguards;
        r.DepartmentId          = req.DepartmentId;
        r.OwnerId               = req.OwnerId;
        return r;
    }

    private static PdplRecordListDto MapList(PdplRecord r) => new(
        r.Id, r.TitleAr, r.TitleEn,
        r.DataCategory.ToString(), r.LegalBasis.ToString(), r.Status.ToString(),
        r.IsThirdPartySharing, r.IsCrossBorderTransfer,
        r.Department?.NameAr, r.Owner?.FullNameAr ?? r.Owner?.Username,
        r.Consents.Count,
        r.DataRequests.Count(d => d.Status == DataRequestStatus.Pending),
        r.LastReviewedAt, r.CreatedAt
    );

    private static PdplRecordDetailDto MapDetail(PdplRecord r) => new(
        r.Id, r.TitleAr, r.TitleEn,
        r.PurposeAr, r.DataSubjectsAr, r.RetentionPeriod, r.SecurityMeasures,
        r.DataCategory.ToString(), r.LegalBasis.ToString(), r.Status.ToString(),
        r.IsThirdPartySharing, r.ThirdPartyDetails,
        r.IsCrossBorderTransfer, r.TransferCountry, r.TransferSafeguards,
        r.Department?.NameAr, r.Owner?.FullNameAr ?? r.Owner?.Username,
        r.OwnerId, r.LastReviewedAt, r.CreatedAt,
        r.Consents.Select(MapConsent).ToList(),
        r.DataRequests.Select(MapRequest).ToList()
    );

    private static PdplConsentDto MapConsent(PdplConsent c) => new(
        c.Id, c.SubjectNameAr, c.SubjectEmail,
        c.IsConsented, c.ConsentedAt, c.WithdrawnAt, c.Notes
    );

    private static PdplDataRequestDto MapRequest(PdplDataRequest d) => new(
        d.Id, d.RequestType.ToString(), d.Status.ToString(),
        d.SubjectNameAr, d.SubjectEmail, d.DetailsAr,
        d.ReceivedAt, d.DueAt, d.ResolvedAt, d.ResolutionAr,
        d.AssignedTo?.FullNameAr ?? d.AssignedTo?.Username,
        d.Status == DataRequestStatus.Pending && DateTime.UtcNow > d.DueAt
    );
}
