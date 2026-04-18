using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Produces("application/json")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _svc;
    private readonly ICurrentUserService _cu;
    public TasksController(ITaskService svc, ICurrentUserService cu) { _svc = svc; _cu = cu; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TaskQuery q)
    {
        q.PageSize = Math.Clamp(q.PageSize, 1, 100);
        var r = await _svc.GetPagedAsync(q);
        return Ok(ApiResponse<PagedResult<TaskListDto>>.Ok(r.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var r = await _svc.GetByIdAsync(id);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<TaskDetailDto>.Ok(r.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveTaskRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        req.Id = 0;
        var r = await _svc.SaveAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return CreatedAtAction(nameof(GetById), new { id = r.Value!.Id }, ApiResponse<TaskDetailDto>.Created(r.Value!));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveTaskRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        req.Id = id;
        var r = await _svc.SaveAsync(req, _cu.Username);
        if (!r.IsSuccess) return BadRequest(ApiResponse<object>.Fail(r.Error!));
        return Ok(ApiResponse<TaskDetailDto>.Ok(r.Value!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _svc.DeleteAsync(id, _cu.Username);
        if (!r.IsSuccess) return NotFound(ApiResponse<object>.NotFound(r.Error!));
        return Ok(ApiResponse<object>.Ok(null, "تم الحذف."));
    }

    [HttpGet("by-risk/{riskId:int}")]
    public async Task<IActionResult> GetByRisk(int riskId)
    {
        var items = await _svc.GetByRiskAsync(riskId);
        return Ok(ApiResponse<List<TaskListDto>>.Ok(items));
    }

    [EnableRateLimiting("export")]
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] TaskQuery q)
    {
        var bytes = await _svc.ExportAsync(q);
        var fileName = $"tasks_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
}
