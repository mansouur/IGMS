namespace IGMS.Application.Common.Models;

// ── List DTO ──────────────────────────────────────────────────────────────────

public class VendorListDto
{
    public int      Id             { get; set; }
    public string   NameAr         { get; set; } = string.Empty;
    public string?  NameEn         { get; set; }
    public string   Type           { get; set; } = string.Empty;
    public string   Status         { get; set; } = string.Empty;
    public string   RiskLevel      { get; set; } = string.Empty;
    public int?     RiskScore      { get; set; }
    public string?  Category       { get; set; }
    public string?  DepartmentName { get; set; }
    public DateTime? ContractEnd   { get; set; }
    public bool     HasNda         { get; set; }
    public bool     HasDataAgreement { get; set; }
    public bool     IsCertified    { get; set; }
    public DateTime? LastAssessedAt { get; set; }
}

// ── Detail DTO ────────────────────────────────────────────────────────────────

public class VendorDetailDto : VendorListDto
{
    public string?   ContactName   { get; set; }
    public string?   ContactEmail  { get; set; }
    public string?   ContactPhone  { get; set; }
    public string?   Website       { get; set; }
    public string?   Notes         { get; set; }
    public DateTime? ContractStart { get; set; }
    public decimal?  ContractValue { get; set; }
    public int?      DepartmentId  { get; set; }
    public string?   RiskNotes     { get; set; }
    public DateTime  CreatedAt     { get; set; }
}

// ── Save request ──────────────────────────────────────────────────────────────

public class SaveVendorRequest
{
    public string   NameAr        { get; set; } = string.Empty;
    public string?  NameEn        { get; set; }
    public string   Type          { get; set; } = "Supplier";
    public string   Status        { get; set; } = "Active";
    public string?  Category      { get; set; }
    public string?  ContactName   { get; set; }
    public string?  ContactEmail  { get; set; }
    public string?  ContactPhone  { get; set; }
    public string?  Website       { get; set; }
    public string?  Notes         { get; set; }
    public DateTime? ContractStart { get; set; }
    public DateTime? ContractEnd  { get; set; }
    public decimal?  ContractValue { get; set; }
    public int?     DepartmentId  { get; set; }
    public bool     HasNda         { get; set; }
    public bool     HasDataAgreement { get; set; }
    public bool     IsCertified   { get; set; }
}

// ── Risk assessment request ───────────────────────────────────────────────────

public class AssessVendorRiskRequest
{
    public string  RiskLevel  { get; set; } = "Low";
    public int?    RiskScore  { get; set; }
    public string? RiskNotes  { get; set; }
}

// ── Query ─────────────────────────────────────────────────────────────────────

public class VendorQuery
{
    public int     Page      { get; set; } = 1;
    public int     PageSize  { get; set; } = 20;
    public string? Search    { get; set; }
    public string? Type      { get; set; }
    public string? Status    { get; set; }
    public string? RiskLevel { get; set; }
}
