using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/kpis")]
[Produces("application/json")]
[Authorize]
public class KpisController : ControllerBase
{
    private readonly IKpiService _svc;
    private readonly IKpiRecordService _records;
    private readonly ICurrentUserService _cu;
    public KpisController(IKpiService svc, IKpiRecordService records, ICurrentUserService cu)
    { _svc = svc; _records = records; _cu = cu; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] KpiQuery q)
    {
        q.PageSize = Math.Clamp(q.PageSize, 1, 100);
        var r = await _svc.GetPagedAsync(q);
        return Ok(ApiResponse<PagedResult<KpiListDto>>.Ok(r.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var r = await _svc.GetByIdAsync(id);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<KpiDetailDto>.Ok(r.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveKpiRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        req.Id = 0;
        var r = await _svc.SaveAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return CreatedAtAction(nameof(GetById), new { id = r.Value!.Id }, ApiResponse<KpiDetailDto>.Created(r.Value!));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveKpiRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        req.Id = id;
        var r = await _svc.SaveAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<KpiDetailDto>.Ok(r.Value!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _svc.DeleteAsync(id, _cu.Username);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<object>.Ok(null, "تم الحذف."));
    }

    // ── KPI Record History ────────────────────────────────────────────────────

    [HttpGet("{kpiId:int}/history")]
    public async Task<IActionResult> GetHistory(int kpiId)
    {
        var list = await _records.GetHistoryAsync(kpiId);
        return Ok(ApiResponse<List<KpiRecordDto>>.Ok(list));
    }

    [HttpPost("{kpiId:int}/history")]
    public async Task<IActionResult> UpsertRecord(int kpiId, [FromBody] AddKpiRecordRequest req)
    {
        req.KpiId = kpiId;
        var r = await _records.UpsertAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<KpiRecordDto>.Ok(r.Value!));
    }

    [HttpDelete("{kpiId:int}/history/{recordId:int}")]
    public async Task<IActionResult> DeleteRecord(int kpiId, int recordId)
    {
        var r = await _records.DeleteAsync(recordId, _cu.Username);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<object>.Ok(null, "تم الحذف."));
    }

    // ── Risk Impact Links ─────────────────────────────────────────────────────
    [HttpGet("{kpiId:int}/risk-links")]
    public async Task<IActionResult> GetRiskLinks(int kpiId)
    {
        var links = await _svc.GetRiskLinksAsync(kpiId);
        return Ok(ApiResponse<List<KpiRiskLinkDto>>.Ok(links));
    }

    [EnableRateLimiting("export")]
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] KpiQuery q)
    {
        var bytes = await _svc.ExportAsync(q);
        var fileName = $"kpis_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
}
