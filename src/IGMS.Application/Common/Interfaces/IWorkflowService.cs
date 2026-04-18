using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Interfaces;

public interface IWorkflowService
{
    // ── Definitions ───────────────────────────────────────────────────────────
    Task<List<WorkflowDefinitionListDto>> GetDefinitionsAsync(string? entityType = null);
    Task<WorkflowDefinitionDetailDto?>    GetDefinitionByIdAsync(int id);
    Task<WorkflowDefinitionDetailDto>     SaveDefinitionAsync(int? id, SaveWorkflowDefinitionRequest req);
    Task                                  DeleteDefinitionAsync(int id);

    // ── Instances ─────────────────────────────────────────────────────────────
    Task<WorkflowInstanceDto?>  SubmitAsync(SubmitWorkflowRequest req, int submittedById);
    Task<WorkflowInstanceDto?>  GetInstanceAsync(string entityType, int entityId);
    Task<List<PendingApprovalDto>> GetPendingAsync(int userId);
    Task<WorkflowInstanceDto?>  ActAsync(int instanceId, ActOnWorkflowRequest req, int actorId);
}
