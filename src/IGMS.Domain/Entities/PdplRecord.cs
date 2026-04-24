using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>Category of personal data as defined by UAE PDPL.</summary>
public enum DataCategory
{
    Basic,        // Name, contact, ID number
    Sensitive,    // Health, biometric, financial, criminal record
    Special,      // Children data, genetic
}

/// <summary>Lawful basis for processing personal data under PDPL Art. 4.</summary>
public enum LegalBasis
{
    Consent,
    ContractPerformance,
    LegalObligation,
    VitalInterests,
    PublicTask,
    LegitimateInterests,
}

public enum PdplRecordStatus { Active, UnderReview, Retired }

/// <summary>
/// Data Processing Record — the central PDPL register (Art. 14).
/// Documents every processing activity for personal data.
/// </summary>
public class PdplRecord : AuditableEntity
{
    public string  TitleAr        { get; set; } = string.Empty;
    public string? TitleEn        { get; set; }
    public string? PurposeAr      { get; set; }   // processing purpose
    public string? DataSubjectsAr { get; set; }   // who: employees, customers, etc.
    public string? RetentionPeriod{ get; set; }   // e.g. "5 سنوات"
    public string? SecurityMeasures{ get; set; }  // technical/org controls applied

    public DataCategory    DataCategory { get; set; } = DataCategory.Basic;
    public LegalBasis      LegalBasis   { get; set; } = LegalBasis.LegalObligation;
    public PdplRecordStatus Status      { get; set; } = PdplRecordStatus.Active;

    public bool IsThirdPartySharing { get; set; }  // data shared with external parties?
    public string? ThirdPartyDetails{ get; set; }

    public bool IsCrossBorderTransfer { get; set; } // transferred outside UAE?
    public string? TransferCountry    { get; set; }
    public string? TransferSafeguards { get; set; } // adequacy decision / SCCs

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? OwnerId { get; set; }              // data owner/controller
    public UserProfile? Owner { get; set; }

    public DateTime? LastReviewedAt { get; set; }

    public List<PdplConsent>      Consents      { get; set; } = [];
    public List<PdplDataRequest>  DataRequests  { get; set; } = [];
}

/// <summary>
/// Consent record — tracks individual opt-in / opt-out decisions (Art. 5-7).
/// </summary>
public class PdplConsent : AuditableEntity
{
    public int PdplRecordId { get; set; }
    public PdplRecord PdplRecord { get; set; } = null!;

    public string  SubjectNameAr  { get; set; } = string.Empty;
    public string? SubjectEmail   { get; set; }
    public string? SubjectIdNumber{ get; set; }  // Emirates ID (hashed/masked)

    public bool    IsConsented    { get; set; } = true;
    public DateTime ConsentedAt   { get; set; } = DateTime.UtcNow;
    public DateTime? WithdrawnAt  { get; set; }
    public string? Notes          { get; set; }
}

/// <summary>
/// Data Subject Request — deletion / correction / access (Art. 13).
/// </summary>
public enum DataRequestType   { Access, Correction, Deletion, Objection, Portability }
public enum DataRequestStatus { Pending, InProgress, Completed, Rejected }

public class PdplDataRequest : AuditableEntity
{
    public int PdplRecordId { get; set; }
    public PdplRecord PdplRecord { get; set; } = null!;

    public DataRequestType   RequestType { get; set; }
    public DataRequestStatus Status      { get; set; } = DataRequestStatus.Pending;

    public string  SubjectNameAr { get; set; } = string.Empty;
    public string? SubjectEmail  { get; set; }
    public string? DetailsAr     { get; set; }

    public DateTime ReceivedAt   { get; set; } = DateTime.UtcNow;
    public DateTime DueAt        { get; set; }  // PDPL: 30 days to respond
    public DateTime? ResolvedAt  { get; set; }
    public string?  ResolutionAr { get; set; }

    public int? AssignedToId { get; set; }
    public UserProfile? AssignedTo { get; set; }
}
