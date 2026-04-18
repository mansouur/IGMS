using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/workflows")]
[Produces("application/json")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService   _svc;
    private readonly ICurrentUserService _cu;
    private readonly TenantDbContext     _db;

    public WorkflowsController(IWorkflowService svc, ICurrentUserService cu, TenantDbContext db)
    {
        _svc = svc;
        _cu  = cu;
        _db  = db;
    }

    // ═══════════════════════════ DEFINITIONS ═════════════════════════════════

    /// <summary>GET /api/v1/workflows/definitions — list all definitions</summary>
    [HttpGet("definitions")]
    public async Task<IActionResult> GetDefinitions([FromQuery] string? entityType = null)
    {
        var result = await _svc.GetDefinitionsAsync(entityType);
        return Ok(ApiResponse<List<WorkflowDefinitionListDto>>.Ok(result));
    }

    /// <summary>GET /api/v1/workflows/definitions/{id}</summary>
    [HttpGet("definitions/{id:int}")]
    public async Task<IActionResult> GetDefinition(int id)
    {
        var dto = await _svc.GetDefinitionByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse<object>.NotFound("تعريف سير العمل غير موجود."));
        return Ok(ApiResponse<WorkflowDefinitionDetailDto>.Ok(dto));
    }

    /// <summary>POST /api/v1/workflows/definitions — create new definition</summary>
    [HttpPost("definitions")]
    public async Task<IActionResult> CreateDefinition([FromBody] SaveWorkflowDefinitionRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));

        if (!IsValidEntityType(req.EntityType))
            return BadRequest(ApiResponse<object>.Fail("نوع العنصر غير صالح. القيم المقبولة: Policy, Risk, ControlTest"));

        var dto = await _svc.SaveDefinitionAsync(null, req);
        return Created($"/api/v1/workflows/definitions/{dto.Id}", ApiResponse<WorkflowDefinitionDetailDto>.Ok(dto));
    }

    /// <summary>PUT /api/v1/workflows/definitions/{id}</summary>
    [HttpPut("definitions/{id:int}")]
    public async Task<IActionResult> UpdateDefinition(int id, [FromBody] SaveWorkflowDefinitionRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));

        if (!IsValidEntityType(req.EntityType))
            return BadRequest(ApiResponse<object>.Fail("نوع العنصر غير صالح."));

        try
        {
            var dto = await _svc.SaveDefinitionAsync(id, req);
            return Ok(ApiResponse<WorkflowDefinitionDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>DELETE /api/v1/workflows/definitions/{id}</summary>
    [HttpDelete("definitions/{id:int}")]
    public async Task<IActionResult> DeleteDefinition(int id)
    {
        try
        {
            await _svc.DeleteDefinitionAsync(id);
            return Ok(ApiResponse<object>.Ok(null));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ═══════════════════════════ INSTANCES ═══════════════════════════════════

    /// <summary>
    /// GET /api/v1/workflows/instances?entityType=Policy&entityId=5
    /// Fetch the latest workflow instance for a given entity.
    /// </summary>
    [HttpGet("instances")]
    public async Task<IActionResult> GetInstance(
        [FromQuery] string entityType,
        [FromQuery] int entityId)
    {
        var dto = await _svc.GetInstanceAsync(entityType, entityId);
        if (dto == null) return Ok(ApiResponse<object>.Ok(null));

        // Enrich CanAct flag
        await EnrichCanAct(dto);
        return Ok(ApiResponse<WorkflowInstanceDto>.Ok(dto));
    }

    /// <summary>GET /api/v1/workflows/instances/pending — approvals waiting for current user</summary>
    [HttpGet("instances/pending")]
    public async Task<IActionResult> GetPending()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _svc.GetPendingAsync(userId.Value);
        return Ok(ApiResponse<List<PendingApprovalDto>>.Ok(result));
    }

    /// <summary>POST /api/v1/workflows/instances — submit entity for approval</summary>
    [HttpPost("instances")]
    public async Task<IActionResult> Submit([FromBody] SubmitWorkflowRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var dto = await _svc.SubmitAsync(req, userId.Value);
            return Ok(ApiResponse<WorkflowInstanceDto>.Ok(dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>POST /api/v1/workflows/instances/{id}/act — approve / reject / comment</summary>
    [HttpPost("instances/{id:int}/act")]
    public async Task<IActionResult> Act(int id, [FromBody] ActOnWorkflowRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var dto = await _svc.ActAsync(id, req, userId.Value);
            if (dto != null) await EnrichCanAct(dto);
            return Ok(ApiResponse<WorkflowInstanceDto>.Ok(dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ─────────────────────────────── Helpers ─────────────────────────────────

    private static readonly HashSet<string> ValidEntityTypes = ["Policy", "Risk", "ControlTest"];
    private static bool IsValidEntityType(string t) => ValidEntityTypes.Contains(t);

    private int? GetCurrentUserId()
    {
        var raw = _cu.UserId;
        return int.TryParse(raw, out var id) ? id : null;
    }

    private async Task EnrichCanAct(WorkflowInstanceDto dto)
    {
        if (dto.Status != "Pending" || dto.CurrentStageOrder == null) return;

        var userId = GetCurrentUserId();
        if (userId == null) return;

        // Get current stage's required role
        var stage = await _db.WorkflowStages
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Join(_db.WorkflowInstances.Where(i => i.Id == dto.Id),
                  s => s.WorkflowDefinitionId,
                  i => i.WorkflowDefinitionId,
                  (s, i) => s)
            .FirstOrDefaultAsync(s => s.StageOrder == dto.CurrentStageOrder);

        if (stage == null) return;

        bool canAct = !stage.RequiredRoleId.HasValue
            || await _db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == stage.RequiredRoleId);

        if (canAct)
        {
            // Check not already acted
            canAct = !await _db.WorkflowInstanceActions
                .AnyAsync(a => a.WorkflowInstanceId == dto.Id
                               && a.StageOrder == dto.CurrentStageOrder
                               && a.ActorId == userId);
        }

        dto.CanAct = canAct;
    }

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
        is { Count: > 0 } errs ? errs : ["طلب غير صالح."];
}
