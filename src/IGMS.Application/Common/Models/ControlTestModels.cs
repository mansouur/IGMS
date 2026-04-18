using System.ComponentModel.DataAnnotations;
using IGMS.Domain.Entities;

namespace IGMS.Application.Common.Models;

public class ControlTestListDto
{
    public int                  Id            { get; set; }
    public string               TitleAr       { get; set; } = string.Empty;
    public string?              TitleEn       { get; set; }
    public string               Code          { get; set; } = string.Empty;
    public string               EntityType    { get; set; } = string.Empty;
    public int                  EntityId      { get; set; }
    public string?              EntityTitleAr { get; set; }
    public ControlEffectiveness Effectiveness { get; set; }
    public DateTime?            TestedAt      { get; set; }
    public DateTime?            NextTestDate  { get; set; }
    public string?              TestedByName  { get; set; }
    public int                  EvidenceCount { get; set; }
    public DateTime             CreatedAt     { get; set; }
}

public class ControlTestDetailDto : ControlTestListDto
{
    public string?                  DescriptionAr { get; set; }
    public string?                  FindingsAr    { get; set; }
    public int?                     TestedById    { get; set; }
    public List<ControlEvidenceDto> Evidences     { get; set; } = [];
}

public class ControlEvidenceDto
{
    public int      Id            { get; set; }
    public string   FileName      { get; set; } = string.Empty;
    public string   ContentType   { get; set; } = string.Empty;
    public long     FileSizeBytes { get; set; }
    public string   UploadedBy    { get; set; } = string.Empty;
    public DateTime UploadedAt    { get; set; }
}

public class SaveControlTestRequest
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string TitleAr { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? TitleEn { get; set; }

    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }

    /// <summary>"Policy" or "Risk"</summary>
    [Required]
    public string EntityType { get; set; } = "Policy";

    [Range(1, int.MaxValue, ErrorMessage = "EntityId مطلوب.")]
    public int EntityId { get; set; }

    public int?                 TestedById    { get; set; }
    public DateTime?            TestedAt      { get; set; }
    public DateTime?            NextTestDate  { get; set; }
    public ControlEffectiveness Effectiveness { get; set; } = ControlEffectiveness.NotTested;
    public string?              FindingsAr    { get; set; }
}

public class ControlTestQuery
{
    public int     Page       { get; set; } = 1;
    public int     PageSize   { get; set; } = 20;
    public string? Search     { get; set; }
    public string? EntityType { get; set; }
    public int?    EntityId   { get; set; }
    public ControlEffectiveness? Effectiveness { get; set; }
}
