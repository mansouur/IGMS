using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/policies")]
[Produces("application/json")]
[Authorize]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _svc;
    private readonly ICurrentUserService _cu;
    public PoliciesController(IPolicyService svc, ICurrentUserService cu) { _svc = svc; _cu = cu; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PolicyQuery q)
    {
        q.PageSize = Math.Clamp(q.PageSize, 1, 100);
        var r = await _svc.GetPagedAsync(q);
        return Ok(ApiResponse<PagedResult<PolicyListDto>>.Ok(r.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var r = await _svc.GetByIdAsync(id);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<PolicyDetailDto>.Ok(r.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SavePolicyRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        req.Id = 0;
        var r = await _svc.SaveAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return CreatedAtAction(nameof(GetById), new { id = r.Value!.Id }, ApiResponse<PolicyDetailDto>.Created(r.Value!));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SavePolicyRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        req.Id = id;
        var r = await _svc.SaveAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<PolicyDetailDto>.Ok(r.Value!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _svc.DeleteAsync(id, _cu.Username);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<object>.Ok(null, "تم الحذف."));
    }

    [EnableRateLimiting("export")]
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] PolicyQuery q)
    {
        var bytes = await _svc.ExportAsync(q);
        var fileName = $"policies_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// تجديد سياسة: ينشئ نسخة مسودة جديدة ويؤرشف الأصل تلقائياً.
    /// </summary>
    [HttpPost("{id:int}/renew")]
    public async Task<IActionResult> Renew(int id)
    {
        var r = await _svc.RenewAsync(id, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<PolicyDetailDto>.Ok(r.Value!));
    }

    [HttpGet("{id:int}/versions")]
    public async Task<IActionResult> GetVersions(int id)
    {
        var versions = await _svc.GetVersionsAsync(id);
        return Ok(ApiResponse<List<PolicyVersionDto>>.Ok(versions));
    }

    /// <summary>
    /// تغيير حالة السياسة. 0=مسودة 1=نشطة 2=مؤرشفة
    /// عند النشر (status=1) يجب إرسال approverId.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetPolicyStatusRequest req)
    {
        var r = await _svc.SetStatusAsync(id, req.Status, _cu.Username, req.ApproverId);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<object>.Ok(null, "تم تحديث الحالة."));
    }

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
}

public record SetPolicyStatusRequest(int Status, int? ApproverId = null);
