using System.ComponentModel.DataAnnotations;
using IGMS.Domain.Entities;

namespace IGMS.Application.Common.Models;

// ── Definition DTOs ───────────────────────────────────────────────────────────

public class WorkflowDefinitionListDto
{
    public int    Id         { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string NameAr     { get; set; } = string.Empty;
    public string NameEn     { get; set; } = string.Empty;
    public bool   IsActive   { get; set; }
    public int    StageCount { get; set; }
}

public class WorkflowStageDto
{
    public int    Id             { get; set; }
    public int    StageOrder     { get; set; }
    public string NameAr         { get; set; } = string.Empty;
    public string NameEn         { get; set; } = string.Empty;
    public int?   RequiredRoleId { get; set; }
    public string? RequiredRoleNameAr { get; set; }
}

public class WorkflowDefinitionDetailDto
{
    public int    Id             { get; set; }
    public string EntityType     { get; set; } = string.Empty;
    public string NameAr         { get; set; } = string.Empty;
    public string NameEn         { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public bool   IsActive       { get; set; }
    public List<WorkflowStageDto> Stages { get; set; } = [];
}

public class SaveWorkflowStageRequest
{
    [Required] public string NameAr     { get; set; } = string.Empty;
    public string NameEn                { get; set; } = string.Empty;
    public int?   RequiredRoleId        { get; set; }
}

public class SaveWorkflowDefinitionRequest
{
    [Required] public string EntityType  { get; set; } = string.Empty;
    [Required] public string NameAr      { get; set; } = string.Empty;
    public string NameEn                 { get; set; } = string.Empty;
    public string? DescriptionAr         { get; set; }
    public bool IsActive                 { get; set; } = true;
    public List<SaveWorkflowStageRequest> Stages { get; set; } = [];
}

// ── Instance DTOs ─────────────────────────────────────────────────────────────

public class WorkflowActionDto
{
    public int    Id          { get; set; }
    public int    StageOrder  { get; set; }
    public string StageNameAr { get; set; } = string.Empty;
    public string ActorName   { get; set; } = string.Empty;
    public string Decision    { get; set; } = string.Empty;
    public string? Comment    { get; set; }
    public DateTime ActedAt   { get; set; }
}

public class WorkflowInstanceDto
{
    public int    Id                  { get; set; }
    public string Status              { get; set; } = string.Empty;
    public int?   CurrentStageOrder   { get; set; }
    public string? CurrentStageNameAr { get; set; }
    public string DefinitionNameAr    { get; set; } = string.Empty;
    public string SubmittedByName     { get; set; } = string.Empty;
    public DateTime SubmittedAt       { get; set; }
    public List<WorkflowActionDto> Actions { get; set; } = [];

    /// <summary>Can the current user act on this instance's current stage?</summary>
    public bool CanAct { get; set; }
}

public class PendingApprovalDto
{
    public int    InstanceId   { get; set; }
    public string EntityType   { get; set; } = string.Empty;
    public int    EntityId     { get; set; }
    public string EntityTitle  { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public string CurrentStage { get; set; } = string.Empty;
    public string SubmittedBy  { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class SubmitWorkflowRequest
{
    [Required] public string EntityType { get; set; } = string.Empty;
    [Required] public int    EntityId   { get; set; }
}

public class ActOnWorkflowRequest
{
    [Required] public string Decision { get; set; } = string.Empty;  // Approved | Rejected | Commented
    public string? Comment { get; set; }
}
