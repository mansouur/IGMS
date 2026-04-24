using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/performance")]
[Produces("application/json")]
[Authorize]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceService  _svc;
    private readonly ICurrentUserService  _cu;

    public PerformanceController(IPerformanceService svc, ICurrentUserService cu)
    {
        _svc = svc;
        _cu  = cu;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PerformanceQuery query)
    {
        var result = await _svc.GetPagedAsync(query);
        return Ok(ApiResponse<PagedResult<PerformanceReviewListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _svc.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse<object>.NotFound("التقييم غير موجود."));
        return Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(dto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SavePerformanceReviewRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        var uid = GetUserId();
        if (uid == null) return Unauthorized();
        try
        {
            var dto = await _svc.CreateAsync(req, uid.Value);
            return Created($"/api/v1/performance/{dto.Id}", ApiResponse<PerformanceReviewDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SavePerformanceReviewRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        try
        {
            var dto = await _svc.UpdateAsync(id, req);
            return Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _svc.DeleteAsync(id); return Ok(ApiResponse<object>.Ok(null)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        try { return Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(await _svc.SubmitAsync(id))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        try { return Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(await _svc.ApproveAsync(id))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectReviewRequest req)
    {
        try { return Ok(ApiResponse<PerformanceReviewDetailDto>.Ok(await _svc.RejectAsync(id, req.Reason))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    private int? GetUserId() =>
        int.TryParse(_cu.UserId, out var id) ? id : null;

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
        is { Count: > 0 } errs ? errs : ["طلب غير صالح."];
}
