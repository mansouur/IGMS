using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/policies/{policyId:int}/acknowledgments")]
[Produces("application/json")]
[Authorize]
public class PolicyAcknowledgmentsController : ControllerBase
{
    private readonly IAcknowledgmentService _svc;
    private readonly ICurrentUserService    _cu;

    public PolicyAcknowledgmentsController(IAcknowledgmentService svc, ICurrentUserService cu)
    {
        _svc = svc;
        _cu  = cu;
    }

    /// <summary>Get acknowledgment status for the current user on this policy.</summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(int policyId)
    {
        if (!int.TryParse(_cu.UserId, out var userId))
            return Unauthorized();

        var status = await _svc.GetStatusAsync(policyId, userId);
        return Ok(ApiResponse<AcknowledgmentStatusDto>.Ok(status));
    }

    /// <summary>Current user acknowledges the policy.</summary>
    [HttpPost]
    public async Task<IActionResult> Acknowledge(int policyId)
    {
        if (!int.TryParse(_cu.UserId, out var userId))
            return Unauthorized();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var r  = await _svc.AcknowledgeAsync(policyId, userId, ip);

        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<AcknowledgmentStatusDto>.Ok(r.Value!));
    }

    /// <summary>List all users who acknowledged this policy (managers only).</summary>
    [HttpGet]
    public async Task<IActionResult> GetRecords(int policyId)
    {
        var records = await _svc.GetRecordsAsync(policyId);
        return Ok(ApiResponse<List<AcknowledgmentRecordDto>>.Ok(records));
    }
}
