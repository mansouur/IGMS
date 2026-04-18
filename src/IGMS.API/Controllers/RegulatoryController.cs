using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/regulatory")]
[Produces("application/json")]
[Authorize]
public class RegulatoryController : ControllerBase
{
    private readonly IRegulatoryService _svc;
    public RegulatoryController(IRegulatoryService svc) => _svc = svc;

    // ── Frameworks ────────────────────────────────────────────────────────────

    /// <summary>GET /api/v1/regulatory/frameworks</summary>
    [HttpGet("frameworks")]
    public async Task<IActionResult> GetFrameworks()
    {
        var result = await _svc.GetFrameworksAsync();
        return Ok(ApiResponse<List<RegulatoryFrameworkDto>>.Ok(result));
    }

    // ── Controls ──────────────────────────────────────────────────────────────

    /// <summary>GET /api/v1/regulatory/frameworks/{id}/controls?domain=</summary>
    [HttpGet("frameworks/{id:int}/controls")]
    public async Task<IActionResult> GetControls(int id, [FromQuery] string? domain = null)
    {
        var result = await _svc.GetControlsByFrameworkAsync(id, domain);
        return Ok(ApiResponse<List<RegulatoryControlDto>>.Ok(result));
    }

    // ── Coverage ──────────────────────────────────────────────────────────────

    /// <summary>GET /api/v1/regulatory/frameworks/{id}/coverage</summary>
    [HttpGet("frameworks/{id:int}/coverage")]
    public async Task<IActionResult> GetCoverage(int id)
    {
        try
        {
            var result = await _svc.GetCoverageAsync(id);
            return Ok(ApiResponse<ComplianceCoverageDto>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.NotFound(ex.Message));
        }
    }

    // ── Mappings ──────────────────────────────────────────────────────────────

    /// <summary>GET /api/v1/regulatory/mappings?entityType=ControlTest&entityId=5</summary>
    [HttpGet("mappings")]
    public async Task<IActionResult> GetMappings(
        [FromQuery] string entityType,
        [FromQuery] int entityId)
    {
        var result = await _svc.GetMappingsForEntityAsync(entityType, entityId);
        return Ok(ApiResponse<List<ControlMappingDto>>.Ok(result));
    }

    /// <summary>POST /api/v1/regulatory/mappings</summary>
    [HttpPost("mappings")]
    public async Task<IActionResult> CreateMapping([FromBody] SaveControlMappingRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));

        try
        {
            var dto = await _svc.CreateMappingAsync(req);
            return Created($"/api/v1/regulatory/mappings/{dto.Id}", ApiResponse<ControlMappingDto>.Ok(dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>PUT /api/v1/regulatory/mappings/{id}</summary>
    [HttpPut("mappings/{id:int}")]
    public async Task<IActionResult> UpdateMapping(int id, [FromBody] UpdateMappingStatusRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));

        try
        {
            var dto = await _svc.UpdateMappingAsync(id, req);
            return Ok(ApiResponse<ControlMappingDto>.Ok(dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>DELETE /api/v1/regulatory/mappings/{id}</summary>
    [HttpDelete("mappings/{id:int}")]
    public async Task<IActionResult> DeleteMapping(int id)
    {
        try
        {
            await _svc.DeleteMappingAsync(id);
            return Ok(ApiResponse<object>.Ok(null));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.NotFound(ex.Message));
        }
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
        is { Count: > 0 } errs ? errs : ["طلب غير صالح."];
}
