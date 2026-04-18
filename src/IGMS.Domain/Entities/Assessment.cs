using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

public enum AssessmentStatus { Draft, Published, Closed }
public enum QuestionType    { YesNo, Rating, Text, MultiChoice }

/// <summary>
/// A survey / self-assessment form distributed to departments or individuals.
/// </summary>
public class Assessment : AuditableEntity
{
    public string TitleAr      { get; set; } = string.Empty;
    public string TitleEn      { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }

    public AssessmentStatus Status { get; set; } = AssessmentStatus.Draft;

    /// <summary>Optional: target a specific department</summary>
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public DateTime? DueDate { get; set; }

    public int? CreatedById { get; set; }
    public UserProfile? CreatedByUser { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public ICollection<AssessmentQuestion>  Questions { get; set; } = [];
    public ICollection<AssessmentResponse>  Responses { get; set; } = [];
}

/// <summary>
/// A single question within an Assessment.
/// Options is a JSON array string for MultiChoice type.
/// </summary>
public class AssessmentQuestion : AuditableEntity
{
    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    public int  QuestionOrder { get; set; }
    public QuestionType QuestionType { get; set; } = QuestionType.YesNo;

    public string  TextAr { get; set; } = string.Empty;
    public string? TextEn { get; set; }

    public bool IsRequired { get; set; } = true;

    /// <summary>JSON array of choice labels for MultiChoice type</summary>
    public string? Options { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public ICollection<AssessmentAnswer> Answers { get; set; } = [];
}

/// <summary>
/// One respondent's submission for an Assessment.
/// </summary>
public class AssessmentResponse : AuditableEntity
{
    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    public int RespondentId { get; set; }
    public UserProfile? Respondent { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public bool IsSubmitted { get; set; } = false;
    public DateTime? SubmittedAt { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public ICollection<AssessmentAnswer> Answers { get; set; } = [];
}

/// <summary>
/// A single answer to one question within a response.
/// AnswerText stores: "Yes"/"No" for YesNo, "1"–"5" for Rating, free text, or option index.
/// </summary>
public class AssessmentAnswer : AuditableEntity
{
    public int AssessmentResponseId  { get; set; }
    public AssessmentResponse? Response { get; set; }

    public int AssessmentQuestionId { get; set; }
    public AssessmentQuestion? Question { get; set; }

    public string? AnswerText { get; set; }
}
