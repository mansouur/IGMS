namespace IGMS.Application.Common.Models;

// ── Consent DTOs ─────────────────────────────────────────────────────────────

public record PdplConsentDto(
    int      Id,
    string   SubjectNameAr,
    string?  SubjectEmail,
    bool     IsConsented,
    DateTime ConsentedAt,
    DateTime? WithdrawnAt,
    string?  Notes
);

// ── Data Request DTOs ─────────────────────────────────────────────────────────

public record PdplDataRequestDto(
    int      Id,
    string   RequestType,
    string   Status,
    string   SubjectNameAr,
    string?  SubjectEmail,
    string?  DetailsAr,
    DateTime ReceivedAt,
    DateTime DueAt,
    DateTime? ResolvedAt,
    string?  ResolutionAr,
    string?  AssignedToName,
    bool     IsOverdue
);

// ── Record DTOs ───────────────────────────────────────────────────────────────

public record PdplRecordListDto(
    int      Id,
    string   TitleAr,
    string?  TitleEn,
    string   DataCategory,
    string   LegalBasis,
    string   Status,
    bool     IsThirdPartySharing,
    bool     IsCrossBorderTransfer,
    string?  DepartmentName,
    string?  OwnerName,
    int      ConsentCount,
    int      PendingRequestCount,
    DateTime? LastReviewedAt,
    DateTime CreatedAt
);

public record PdplRecordDetailDto(
    int      Id,
    string   TitleAr,
    string?  TitleEn,
    string?  PurposeAr,
    string?  DataSubjectsAr,
    string?  RetentionPeriod,
    string?  SecurityMeasures,
    string   DataCategory,
    string   LegalBasis,
    string   Status,
    bool     IsThirdPartySharing,
    string?  ThirdPartyDetails,
    bool     IsCrossBorderTransfer,
    string?  TransferCountry,
    string?  TransferSafeguards,
    string?  DepartmentName,
    string?  OwnerName,
    int?     OwnerId,
    DateTime? LastReviewedAt,
    DateTime CreatedAt,
    List<PdplConsentDto>     Consents,
    List<PdplDataRequestDto> DataRequests
);

// ── Requests ──────────────────────────────────────────────────────────────────

public class SavePdplRecordRequest
{
    public string  TitleAr              { get; set; } = string.Empty;
    public string? TitleEn              { get; set; }
    public string? PurposeAr            { get; set; }
    public string? DataSubjectsAr       { get; set; }
    public string? RetentionPeriod      { get; set; }
    public string? SecurityMeasures     { get; set; }
    public string  DataCategory         { get; set; } = "Basic";
    public string  LegalBasis           { get; set; } = "LegalObligation";
    public string  Status               { get; set; } = "Active";
    public bool    IsThirdPartySharing  { get; set; }
    public string? ThirdPartyDetails    { get; set; }
    public bool    IsCrossBorderTransfer{ get; set; }
    public string? TransferCountry      { get; set; }
    public string? TransferSafeguards   { get; set; }
    public int?    DepartmentId         { get; set; }
    public int?    OwnerId              { get; set; }
}

public class SaveConsentRequest
{
    public string  SubjectNameAr   { get; set; } = string.Empty;
    public string? SubjectEmail    { get; set; }
    public string? SubjectIdNumber { get; set; }
    public bool    IsConsented     { get; set; } = true;
    public string? Notes           { get; set; }
}

public class SaveDataRequestRequest
{
    public string  RequestType   { get; set; } = "Access";
    public string  SubjectNameAr { get; set; } = string.Empty;
    public string? SubjectEmail  { get; set; }
    public string? DetailsAr     { get; set; }
    public int?    AssignedToId  { get; set; }
}

public class ResolveDataRequestRequest
{
    public string  ResolutionAr { get; set; } = string.Empty;
    public bool    Rejected     { get; set; }
}

// ── Query ─────────────────────────────────────────────────────────────────────

public class PdplQuery
{
    public int     Page         { get; set; } = 1;
    public int     PageSize     { get; set; } = 20;
    public string? Search       { get; set; }
    public string? Status       { get; set; }
    public string? DataCategory { get; set; }
    public int?    DepartmentId { get; set; }
}

public class PdplRequestQuery
{
    public int     Page        { get; set; } = 1;
    public int     PageSize    { get; set; } = 20;
    public string? Status      { get; set; }
    public string? RequestType { get; set; }
}
