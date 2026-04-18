using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/compliance")]
[Produces("application/json")]
[Authorize]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceMappingService _svc;
    private readonly ICurrentUserService _cu;
    public ComplianceController(IComplianceMappingService svc, ICurrentUserService cu)
    { _svc = svc; _cu = cu; }

    /// <summary>GET /api/v1/compliance?entityType=Policy&entityId=5</summary>
    [HttpGet]
    public async Task<IActionResult> GetByEntity([FromQuery] string entityType, [FromQuery] int entityId)
    {
        var list = await _svc.GetByEntityAsync(entityType, entityId);
        return Ok(ApiResponse<List<ComplianceMappingDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddComplianceMappingRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        var r = await _svc.AddAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<ComplianceMappingDto>.Ok(r.Value!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _svc.DeleteAsync(id, _cu.Username);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<object>.Ok(null, "تم الحذف."));
    }

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
}
