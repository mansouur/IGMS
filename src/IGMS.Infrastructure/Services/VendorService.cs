using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly TenantDbContext _db;
    public VendorService(TenantDbContext db) => _db = db;

    // ── List ──────────────────────────────────────────────────────────────────

    public async Task<PagedResult<VendorListDto>> GetPagedAsync(VendorQuery query)
    {
        var q = _db.Vendors
            .AsNoTracking()
            .Include(v => v.Department)
            .Where(v => !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(v => v.NameAr.Contains(query.Search) ||
                              (v.NameEn != null && v.NameEn.Contains(query.Search)) ||
                              (v.Category != null && v.Category.Contains(query.Search)));

        if (!string.IsNullOrEmpty(query.Type) &&
            Enum.TryParse<VendorType>(query.Type, out var vt))
            q = q.Where(v => v.Type == vt);

        if (!string.IsNullOrEmpty(query.Status) &&
            Enum.TryParse<VendorStatus>(query.Status, out var vs))
            q = q.Where(v => v.Status == vs);

        if (!string.IsNullOrEmpty(query.RiskLevel) &&
            Enum.TryParse<VendorRiskLevel>(query.RiskLevel, out var rl))
            q = q.Where(v => v.RiskLevel == rl);

        var total = await q.CountAsync();
        var items = await q
            .OrderBy(v => v.RiskLevel == VendorRiskLevel.Critical ? 0
                        : v.RiskLevel == VendorRiskLevel.High     ? 1
                        : v.RiskLevel == VendorRiskLevel.Medium   ? 2 : 3)
            .ThenBy(v => v.NameAr)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<VendorListDto>
        {
            Items       = items.Select(MapList).ToList(),
            TotalCount  = total,
            CurrentPage = query.Page,
            PageSize    = query.PageSize,
        };
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    public async Task<VendorDetailDto?> GetByIdAsync(int id)
    {
        var v = await _db.Vendors
            .AsNoTracking()
            .Include(v => v.Department)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        return v == null ? null : MapDetail(v);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<VendorDetailDto> CreateAsync(SaveVendorRequest req)
    {
        if (!Enum.TryParse<VendorType>(req.Type, out var type))
            throw new InvalidOperationException($"نوع مورد غير صالح: {req.Type}");
        if (!Enum.TryParse<VendorStatus>(req.Status, out var status))
            throw new InvalidOperationException($"حالة غير صالحة: {req.Status}");

        var vendor = new Vendor
        {
            NameAr           = req.NameAr,
            NameEn           = req.NameEn,
            Type             = type,
            Status           = status,
            Category         = req.Category,
            ContactName      = req.ContactName,
            ContactEmail     = req.ContactEmail,
            ContactPhone     = req.ContactPhone,
            Website          = req.Website,
            Notes            = req.Notes,
            ContractStart    = req.ContractStart,
            ContractEnd      = req.ContractEnd,
            ContractValue    = req.ContractValue,
            DepartmentId     = req.DepartmentId,
            HasNda           = req.HasNda,
            HasDataAgreement = req.HasDataAgreement,
            IsCertified      = req.IsCertified,
            CreatedAt        = DateTime.UtcNow,
            CreatedBy        = "api",
        };

        _db.Vendors.Add(vendor);
        await _db.SaveChangesAsync();

        return MapDetail((await LoadFull(vendor.Id))!);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<VendorDetailDto> UpdateAsync(int id, SaveVendorRequest req)
    {
        var vendor = await _db.Vendors.Include(v => v.Department)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted)
            ?? throw new InvalidOperationException("المورد غير موجود.");

        if (!Enum.TryParse<VendorType>(req.Type, out var type))
            throw new InvalidOperationException($"نوع مورد غير صالح: {req.Type}");
        if (!Enum.TryParse<VendorStatus>(req.Status, out var status))
            throw new InvalidOperationException($"حالة غير صالحة: {req.Status}");

        vendor.NameAr           = req.NameAr;
        vendor.NameEn           = req.NameEn;
        vendor.Type             = type;
        vendor.Status           = status;
        vendor.Category         = req.Category;
        vendor.ContactName      = req.ContactName;
        vendor.ContactEmail     = req.ContactEmail;
        vendor.ContactPhone     = req.ContactPhone;
        vendor.Website          = req.Website;
        vendor.Notes            = req.Notes;
        vendor.ContractStart    = req.ContractStart;
        vendor.ContractEnd      = req.ContractEnd;
        vendor.ContractValue    = req.ContractValue;
        vendor.DepartmentId     = req.DepartmentId;
        vendor.HasNda           = req.HasNda;
        vendor.HasDataAgreement = req.HasDataAgreement;
        vendor.IsCertified      = req.IsCertified;
        vendor.ModifiedAt       = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(id))!);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task DeleteAsync(int id)
    {
        var vendor = await _db.Vendors.FindAsync(id)
            ?? throw new InvalidOperationException("المورد غير موجود.");
        vendor.IsDeleted = true;
        vendor.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Risk Assessment ───────────────────────────────────────────────────────

    public async Task<VendorDetailDto> AssessRiskAsync(int id, AssessVendorRiskRequest req)
    {
        var vendor = await _db.Vendors.FindAsync(id)
            ?? throw new InvalidOperationException("المورد غير موجود.");

        if (!Enum.TryParse<VendorRiskLevel>(req.RiskLevel, out var rl))
            throw new InvalidOperationException($"مستوى مخاطرة غير صالح: {req.RiskLevel}");

        vendor.RiskLevel      = rl;
        vendor.RiskScore      = req.RiskScore;
        vendor.RiskNotes      = req.RiskNotes;
        vendor.LastAssessedAt = DateTime.UtcNow;
        vendor.ModifiedAt     = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapDetail((await LoadFull(id))!);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Vendor?> LoadFull(int id) =>
        await _db.Vendors.AsNoTracking()
            .Include(v => v.Department)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

    private static VendorListDto MapList(Vendor v) => new()
    {
        Id               = v.Id,
        NameAr           = v.NameAr,
        NameEn           = v.NameEn,
        Type             = v.Type.ToString(),
        Status           = v.Status.ToString(),
        RiskLevel        = v.RiskLevel.ToString(),
        RiskScore        = v.RiskScore,
        Category         = v.Category,
        DepartmentName   = v.Department?.NameAr,
        ContractEnd      = v.ContractEnd,
        HasNda           = v.HasNda,
        HasDataAgreement = v.HasDataAgreement,
        IsCertified      = v.IsCertified,
        LastAssessedAt   = v.LastAssessedAt,
    };

    private static VendorDetailDto MapDetail(Vendor v) => new()
    {
        Id               = v.Id,
        NameAr           = v.NameAr,
        NameEn           = v.NameEn,
        Type             = v.Type.ToString(),
        Status           = v.Status.ToString(),
        RiskLevel        = v.RiskLevel.ToString(),
        RiskScore        = v.RiskScore,
        Category         = v.Category,
        DepartmentName   = v.Department?.NameAr,
        DepartmentId     = v.DepartmentId,
        ContractStart    = v.ContractStart,
        ContractEnd      = v.ContractEnd,
        ContractValue    = v.ContractValue,
        ContactName      = v.ContactName,
        ContactEmail     = v.ContactEmail,
        ContactPhone     = v.ContactPhone,
        Website          = v.Website,
        Notes            = v.Notes,
        HasNda           = v.HasNda,
        HasDataAgreement = v.HasDataAgreement,
        IsCertified      = v.IsCertified,
        LastAssessedAt   = v.LastAssessedAt,
        RiskNotes        = v.RiskNotes,
        CreatedAt        = v.CreatedAt,
    };
}
