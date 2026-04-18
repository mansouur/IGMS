using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/assessments")]
[Produces("application/json")]
[Authorize]
public class AssessmentsController : ControllerBase
{
    private readonly IAssessmentService  _svc;
    private readonly ICurrentUserService _cu;

    public AssessmentsController(IAssessmentService svc, ICurrentUserService cu)
    {
        _svc = svc;
        _cu  = cu;
    }

    // ── List ──────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var uid = GetUserId();
        if (uid == null) return Unauthorized();
        var result = await _svc.GetListAsync(uid.Value);
        return Ok(ApiResponse<List<AssessmentListDto>>.Ok(result));
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _svc.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse<object>.NotFound("الاستبيان غير موجود."));
        return Ok(ApiResponse<AssessmentDetailDto>.Ok(dto));
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveAssessmentRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        var uid = GetUserId();
        if (uid == null) return Unauthorized();
        var dto = await _svc.SaveAsync(null, req, uid.Value);
        return Created($"/api/v1/assessments/{dto.Id}", ApiResponse<AssessmentDetailDto>.Ok(dto));
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveAssessmentRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        var uid = GetUserId();
        if (uid == null) return Unauthorized();
        try
        {
            var dto = await _svc.SaveAsync(id, req, uid.Value);
            return Ok(ApiResponse<AssessmentDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _svc.DeleteAsync(id); return Ok(ApiResponse<object>.Ok(null)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    // ── Publish / Close ───────────────────────────────────────────────────────

    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        try { await _svc.PublishAsync(id); return Ok(ApiResponse<object>.Ok(null)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id)
    {
        try { await _svc.CloseAsync(id); return Ok(ApiResponse<object>.Ok(null)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    // ── My Response ───────────────────────────────────────────────────────────

    [HttpGet("{id:int}/my-response")]
    public async Task<IActionResult> GetMyResponse(int id)
    {
        var uid = GetUserId();
        if (uid == null) return Unauthorized();
        var dto = await _svc.GetMyResponseAsync(id, uid.Value);
        return Ok(ApiResponse<AssessmentResponseDto?>.Ok(dto));
    }

    [HttpPost("{id:int}/respond")]
    public async Task<IActionResult> Respond(int id, [FromBody] SubmitResponseRequest req,
        [FromQuery] bool submit = false, [FromQuery] int? departmentId = null)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        var uid = GetUserId();
        if (uid == null) return Unauthorized();
        try
        {
            var dto = await _svc.SaveResponseAsync(id, uid.Value, departmentId, req, submit);
            return Ok(ApiResponse<AssessmentResponseDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    // ── Report ────────────────────────────────────────────────────────────────

    [HttpGet("{id:int}/report")]
    public async Task<IActionResult> GetReport(int id)
    {
        try
        {
            var dto = await _svc.GetReportAsync(id);
            return Ok(ApiResponse<AssessmentReportDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return NotFound(ApiResponse<object>.NotFound(ex.Message)); }
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private int? GetUserId() =>
        int.TryParse(_cu.UserId, out var id) ? id : null;

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
        is { Count: > 0 } errs ? errs : ["طلب غير صالح."];
}
