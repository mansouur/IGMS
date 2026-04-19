using System.ComponentModel.DataAnnotations;

namespace IGMS.Application.Common.Models;

// ── Assessment DTOs ───────────────────────────────────────────────────────────

public class AssessmentListDto
{
    public int    Id             { get; set; }
    public string TitleAr        { get; set; } = string.Empty;
    public string TitleEn        { get; set; } = string.Empty;
    public string Status         { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public DateTime? DueDate     { get; set; }
    public int    QuestionCount  { get; set; }
    public int    ResponseCount  { get; set; }
    public int    SubmittedCount { get; set; }
    public bool   MyResponseSubmitted { get; set; }
}

public class AssessmentQuestionDto
{
    public int    Id            { get; set; }
    public int    QuestionOrder { get; set; }
    public string QuestionType  { get; set; } = string.Empty;
    public string TextAr        { get; set; } = string.Empty;
    public string? TextEn       { get; set; }
    public bool   IsRequired    { get; set; }
    public List<string> Options { get; set; } = [];
}

public class AssessmentDetailDto
{
    public int    Id             { get; set; }
    public string TitleAr        { get; set; } = string.Empty;
    public string TitleEn        { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string Status         { get; set; } = string.Empty;
    public int?   DepartmentId   { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime? DueDate     { get; set; }
    public List<AssessmentQuestionDto> Questions { get; set; } = [];
    public int    ResponseCount  { get; set; }
    public int    SubmittedCount { get; set; }
}

// ── Save requests ─────────────────────────────────────────────────────────────

public class SaveQuestionRequest
{
    [Required] public string QuestionType { get; set; } = "YesNo";
    [Required] public string TextAr       { get; set; } = string.Empty;
    public string? TextEn                 { get; set; }
    public bool IsRequired                { get; set; } = true;
    public List<string> Options           { get; set; } = [];
}

public class SaveAssessmentRequest
{
    [Required] public string TitleAr     { get; set; } = string.Empty;
    public string TitleEn                { get; set; } = string.Empty;
    public string? DescriptionAr         { get; set; }
    public int?   DepartmentId           { get; set; }
    public DateTime? DueDate             { get; set; }
    public List<SaveQuestionRequest> Questions { get; set; } = [];
}

// ── Response DTOs ─────────────────────────────────────────────────────────────

public class AssessmentAnswerDto
{
    public int    QuestionId   { get; set; }
    public string? AnswerText  { get; set; }
}

public class AssessmentResponseDto
{
    public int    Id          { get; set; }
    public int    AssessmentId { get; set; }
    public string RespondentName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public bool   IsSubmitted { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<AssessmentAnswerDto> Answers { get; set; } = [];
}

public class SubmitResponseRequest
{
    [Required] public List<AssessmentAnswerDto> Answers { get; set; } = [];
}

// ── Report ────────────────────────────────────────────────────────────────────

public class AssessmentReportDto
{
    public int    AssessmentId  { get; set; }
    public string TitleAr       { get; set; } = string.Empty;
    public int    TotalInvited  { get; set; }
    public int    TotalResponded { get; set; }
    public double ResponseRate  { get; set; }
    public List<QuestionReportDto> Questions { get; set; } = [];
}

public class QuestionReportDto
{
    public int    QuestionId   { get; set; }
    public string TextAr       { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public int    AnswerCount  { get; set; }
    /// <summary>For YesNo: {"Yes":5,"No":3}, for Rating: {"1":2,"5":8}, for MultiChoice: {"Option A":4}</summary>
    public Dictionary<string, int> Distribution { get; set; } = [];
    /// <summary>For Rating: average score</summary>
    public double? Average { get; set; }
    /// <summary>For Text: first 5 responses</summary>
    public List<string> TextSamples { get; set; } = [];
}

// ── Query ─────────────────────────────────────────────────────────────────────

public class AssessmentQuery
{
    public int     Page     { get; set; } = 1;
    public int     PageSize { get; set; } = 20;
    public string? Search   { get; set; }
    public string? Status   { get; set; }
}
