using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/pdpl")]
[Produces("application/json")]
[Authorize]
public class PdplController : ControllerBase
{
    private readonly IPdplService        _svc;
    private readonly ICurrentUserService _cu;

    public PdplController(IPdplService svc, ICurrentUserService cu)
    {
        _svc = svc;
        _cu  = cu;
    }

    // ── Records ───────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PdplQuery query)
    {
        var result = await _svc.GetPagedAsync(query);
        return Ok(ApiResponse<PagedResult<PdplRecordListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _svc.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse<object>.NotFound("السجل غير موجود."));
        return Ok(ApiResponse<PdplRecordDetailDto>.Ok(dto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SavePdplRecordRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        var uid = GetUserId(); if (uid == null) return Unauthorized();
        try
        {
            var dto = await _svc.CreateAsync(req, uid.Value);
            return Created($"/api/v1/pdpl/{dto.Id}", ApiResponse<PdplRecordDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SavePdplRecordRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        try { return Ok(ApiResponse<PdplRecordDetailDto>.Ok(await _svc.UpdateAsync(id, req))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _svc.DeleteAsync(id); return Ok(ApiResponse<object>.Ok(null)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/review")]
    public async Task<IActionResult> MarkReviewed(int id)
    {
        try { return Ok(ApiResponse<PdplRecordDetailDto>.Ok(await _svc.MarkReviewedAsync(id))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    // ── Consents ──────────────────────────────────────────────────────────────

    [HttpPost("{id:int}/consents")]
    public async Task<IActionResult> AddConsent(int id, [FromBody] SaveConsentRequest req)
    {
        try { return Ok(ApiResponse<PdplConsentDto>.Ok(await _svc.AddConsentAsync(id, req))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/consents/{consentId:int}/withdraw")]
    public async Task<IActionResult> WithdrawConsent(int id, int consentId)
    {
        try { return Ok(ApiResponse<PdplConsentDto>.Ok(await _svc.WithdrawConsentAsync(id, consentId))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    // ── Data Requests ─────────────────────────────────────────────────────────

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests([FromQuery] PdplRequestQuery query)
    {
        var result = await _svc.GetRequestsAsync(query);
        return Ok(ApiResponse<PagedResult<PdplDataRequestDto>>.Ok(result));
    }

    [HttpPost("{id:int}/requests")]
    public async Task<IActionResult> AddRequest(int id, [FromBody] SaveDataRequestRequest req)
    {
        var uid = GetUserId(); if (uid == null) return Unauthorized();
        try { return Ok(ApiResponse<PdplDataRequestDto>.Ok(await _svc.AddRequestAsync(id, req, uid.Value))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/requests/{requestId:int}/resolve")]
    public async Task<IActionResult> ResolveRequest(int id, int requestId, [FromBody] ResolveDataRequestRequest req)
    {
        try { return Ok(ApiResponse<PdplDataRequestDto>.Ok(await _svc.ResolveRequestAsync(id, requestId, req))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    private int? GetUserId() => int.TryParse(_cu.UserId, out var id) ? id : null;

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
        is { Count: > 0 } errs ? errs : ["طلب غير صالح."];
}
