using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public class Policy : AuditableEntity
{
    public string  TitleAr       { get; set; } = string.Empty;
    public string  TitleEn       { get; set; } = string.Empty;
    public string  Code          { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }

    public PolicyCategory Category { get; set; } = PolicyCategory.Governance;
    public PolicyStatus   Status   { get; set; } = PolicyStatus.Draft;

    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate    { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? OwnerId { get; set; }
    public UserProfile? Owner { get; set; }

    /// <summary>من اعتمد السياسة رسمياً — يُسجَّل تلقائياً عند النشر</summary>
    public int? ApproverId { get; set; }
    public UserProfile? Approver { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<PolicyAttachment> Attachments { get; set; } = [];
}

public enum PolicyCategory
{
    Governance   = 0,
    IT           = 1,
    HR           = 2,
    Financial    = 3,
    Operations   = 4,
}

public enum PolicyStatus
{
    Draft    = 0,
    Active   = 1,
    Archived = 2,
}
