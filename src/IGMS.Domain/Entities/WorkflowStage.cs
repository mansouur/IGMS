using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// A single approval step within a WorkflowDefinition.
/// Stages are executed in ascending StageOrder.
/// RequiredRoleId determines which role's members can approve this stage.
/// </summary>
public class WorkflowStage : AuditableEntity
{
    public int WorkflowDefinitionId { get; set; }
    public WorkflowDefinition? Definition { get; set; }

    /// <summary>1-based order of execution</summary>
    public int StageOrder { get; set; }

    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    /// <summary>Role whose members can act on this stage (null = any authenticated user)</summary>
    public int? RequiredRoleId { get; set; }
    public Role? RequiredRole { get; set; }
}
