using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/audit-logs")]
[Produces("application/json")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _svc;
    public AuditLogsController(IAuditLogService svc) => _svc = svc;

    /// <summary>Paginated audit trail with optional filters.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AuditLogQuery q)
    {
        q.PageSize = Math.Clamp(q.PageSize, 1, 100);
        var result = await _svc.GetPagedAsync(q);
        return Ok(ApiResponse<PagedResult<AuditLogListDto>>.Ok(result));
    }

    /// <summary>Returns list of entity types that have audit records (for filter dropdown).</summary>
    [HttpGet("entity-types")]
    public async Task<IActionResult> GetEntityTypes()
    {
        var types = await _svc.GetEntityTypesAsync();
        return Ok(ApiResponse<List<string>>.Ok(types));
    }
}
