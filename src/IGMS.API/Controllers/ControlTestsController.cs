using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/control-tests")]
[Produces("application/json")]
[Authorize]
public class ControlTestsController : ControllerBase
{
    private readonly IControlTestService _svc;
    private readonly ICurrentUserService _cu;
    private readonly TenantContext       _tenant;

    public ControlTestsController(
        IControlTestService svc,
        ICurrentUserService cu,
        TenantContext tenant)
    {
        _svc    = svc;
        _cu     = cu;
        _tenant = tenant;
    }

    // ── List ──────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ControlTestQuery q)
    {
        q.PageSize = Math.Clamp(q.PageSize, 1, 100);
        var r = await _svc.GetPagedAsync(q);
        return Ok(ApiResponse<PagedResult<ControlTestListDto>>.Ok(r.Value!));
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var r = await _svc.GetByIdAsync(id);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<ControlTestDetailDto>.Ok(r.Value!));
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveControlTestRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        req.Id = 0;
        var r = await _svc.SaveAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return CreatedAtAction(nameof(GetById), new { id = r.Value!.Id },
            ApiResponse<ControlTestDetailDto>.Created(r.Value!));
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveControlTestRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        req.Id = id;
        var r = await _svc.SaveAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<ControlTestDetailDto>.Ok(r.Value!));
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _svc.DeleteAsync(id, _cu.Username);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<object>.Ok(null, "تم الحذف."));
    }

    // ── Evidence: Upload ──────────────────────────────────────────────────────

    [HttpPost("{id:int}/evidence")]
    [RequestSizeLimit(21_000_000)]
    public async Task<IActionResult> UploadEvidence(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("لم يتم إرفاق ملف."));

        await using var stream = file.OpenReadStream();
        var r = await _svc.UploadEvidenceAsync(
            id, stream, file.FileName, file.ContentType,
            file.Length, _tenant.TenantKey, _cu.Username);

        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<ControlEvidenceDto>.Ok(r.Value!));
    }

    // ── Evidence: Download ────────────────────────────────────────────────────

    [HttpGet("{id:int}/evidence/{evidenceId:int}")]
    public async Task<IActionResult> DownloadEvidence(int id, int evidenceId)
    {
        var r = await _svc.DownloadEvidenceAsync(evidenceId);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return File(r.Value!.Data, r.Value!.ContentType, r.Value!.FileName);
    }

    // ── Evidence: Delete ──────────────────────────────────────────────────────

    [HttpDelete("{id:int}/evidence/{evidenceId:int}")]
    public async Task<IActionResult> DeleteEvidence(int id, int evidenceId)
    {
        var r = await _svc.DeleteEvidenceAsync(evidenceId);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<object>.Ok(null, "تم حذف الدليل."));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
}
