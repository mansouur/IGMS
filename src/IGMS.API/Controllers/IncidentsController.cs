using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/incidents")]
[Produces("application/json")]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService  _svc;
    private readonly ICurrentUserService _cu;

    public IncidentsController(IIncidentService svc, ICurrentUserService cu)
    {
        _svc = svc;
        _cu  = cu;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] IncidentQuery query)
    {
        var result = await _svc.GetPagedAsync(query);
        return Ok(ApiResponse<PagedResult<IncidentListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _svc.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse<object>.NotFound("الحادثة غير موجودة."));
        return Ok(ApiResponse<IncidentDetailDto>.Ok(dto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveIncidentRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        var uid = GetUserId();
        if (uid == null) return Unauthorized();
        try
        {
            var dto = await _svc.CreateAsync(req, uid.Value);
            return Created($"/api/v1/incidents/{dto.Id}", ApiResponse<IncidentDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveIncidentRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        try
        {
            var dto = await _svc.UpdateAsync(id, req);
            return Ok(ApiResponse<IncidentDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _svc.DeleteAsync(id); return Ok(ApiResponse<object>.Ok(null)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? status, [FromQuery] string? severity)
    {
        var bytes = await _svc.ExportAsync(status, severity);
        var fileName = $"incidents_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost("{id:int}/resolve")]
    public async Task<IActionResult> Resolve(int id, [FromBody] ResolveIncidentRequest req)
    {
        try
        {
            var dto = await _svc.ResolveAsync(id, req.ResolutionNotes);
            return Ok(ApiResponse<IncidentDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    private int? GetUserId() =>
        int.TryParse(_cu.UserId, out var id) ? id : null;

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
        is { Count: > 0 } errs ? errs : ["طلب غير صالح."];
}

public class ResolveIncidentRequest
{
    public string? ResolutionNotes { get; set; }
}
