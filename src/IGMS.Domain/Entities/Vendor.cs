using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public enum VendorType   { Supplier, Partner, Consultant, Contractor, CloudProvider }
public enum VendorStatus { Active, Inactive, UnderReview, Blacklisted }
public enum VendorRiskLevel { Low, Medium, High, Critical }

/// <summary>
/// A third-party vendor / supplier / partner whose risk is tracked as part of IT governance.
/// </summary>
public class Vendor : AuditableEntity
{
    public string  NameAr        { get; set; } = string.Empty;
    public string? NameEn        { get; set; }

    public VendorType   Type   { get; set; } = VendorType.Supplier;
    public VendorStatus Status { get; set; } = VendorStatus.Active;

    public string? Category     { get; set; }  // e.g. تقنية المعلومات، قانوني، مالي
    public string? ContactName  { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Website      { get; set; }
    public string? Notes        { get; set; }

    // ── Contract ──────────────────────────────────────────────────────────────
    public DateTime? ContractStart { get; set; }
    public DateTime? ContractEnd   { get; set; }
    public decimal?  ContractValue { get; set; }

    // ── Risk ──────────────────────────────────────────────────────────────────
    public VendorRiskLevel RiskLevel     { get; set; } = VendorRiskLevel.Low;
    public int?            RiskScore     { get; set; }  // 1–25 — مثل مصفوفة المخاطر
    public DateTime?       LastAssessedAt { get; set; }
    public string?         RiskNotes     { get; set; }

    // ── Compliance flags ──────────────────────────────────────────────────────
    public bool HasNda          { get; set; }
    public bool HasDataAgreement { get; set; }
    public bool IsCertified     { get; set; }  // ISO/SOC2 شهادة

    // ── Optional department linkage ───────────────────────────────────────────
    public int?         DepartmentId { get; set; }
    public Department?  Department   { get; set; }
}
