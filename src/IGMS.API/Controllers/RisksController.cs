using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/risks")]
[Produces("application/json")]
[Authorize]
public class RisksController : ControllerBase
{
    private readonly IRiskService _svc;
    private readonly ICurrentUserService _cu;
    public RisksController(IRiskService svc, ICurrentUserService cu) { _svc = svc; _cu = cu; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] RiskQuery q)
    {
        q.PageSize = Math.Clamp(q.PageSize, 1, 100);
        var r = await _svc.GetPagedAsync(q);
        return Ok(ApiResponse<PagedResult<RiskListDto>>.Ok(r.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var r = await _svc.GetByIdAsync(id);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<RiskDetailDto>.Ok(r.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveRiskRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        req.Id = 0;
        var r = await _svc.SaveAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return CreatedAtAction(nameof(GetById), new { id = r.Value!.Id }, ApiResponse<RiskDetailDto>.Created(r.Value!));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveRiskRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        req.Id = id;
        var r = await _svc.SaveAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<RiskDetailDto>.Ok(r.Value!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _svc.DeleteAsync(id, _cu.Username);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<object>.Ok(null, "تم الحذف."));
    }

    [HttpGet("heatmap")]
    public async Task<IActionResult> GetHeatMap()
    {
        var items = await _svc.GetHeatMapAsync();
        return Ok(ApiResponse<List<RiskHeatMapItemDto>>.Ok(items));
    }

    // ── KPI Impact Links ──────────────────────────────────────────────────────
    [HttpGet("{riskId:int}/kpi-links")]
    public async Task<IActionResult> GetKpiLinks(int riskId)
    {
        var links = await _svc.GetKpiLinksAsync(riskId);
        return Ok(ApiResponse<List<RiskKpiLinkDto>>.Ok(links));
    }

    [HttpPost("{riskId:int}/kpi-links")]
    public async Task<IActionResult> AddKpiLink(int riskId, [FromBody] AddRiskKpiLinkRequest req)
    {
        try
        {
            var link = await _svc.AddKpiLinkAsync(riskId, req.KpiId, req.Notes);
            return Ok(ApiResponse<RiskKpiLinkDto>.Ok(link));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("kpi-links/{mappingId:int}")]
    public async Task<IActionResult> RemoveKpiLink(int mappingId)
    {
        await _svc.RemoveKpiLinkAsync(mappingId);
        return Ok(ApiResponse<object>.Ok(null, "تم إزالة الربط."));
    }

    [EnableRateLimiting("export")]
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] RiskQuery q)
    {
        var bytes = await _svc.ExportAsync(q);
        var fileName = $"risks_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
}
