using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/meetings")]
[Produces("application/json")]
[Authorize]
public class MeetingsController : ControllerBase
{
    private readonly IMeetingService   _svc;
    private readonly ICurrentUserService _cu;

    public MeetingsController(IMeetingService svc, ICurrentUserService cu)
    {
        _svc = svc;
        _cu  = cu;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] MeetingQuery query)
    {
        var uid    = GetUserId() ?? 0;
        var result = await _svc.GetPagedAsync(query, uid);
        return Ok(ApiResponse<PagedResult<MeetingListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _svc.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse<object>.NotFound("الاجتماع غير موجود."));
        return Ok(ApiResponse<MeetingDetailDto>.Ok(dto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveMeetingRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        var uid = GetUserId();
        if (uid == null) return Unauthorized();
        try
        {
            var dto = await _svc.CreateAsync(req, uid.Value);
            return Created($"/api/v1/meetings/{dto.Id}", ApiResponse<MeetingDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveMeetingRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        try
        {
            var dto = await _svc.UpdateAsync(id, req);
            return Ok(ApiResponse<MeetingDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _svc.DeleteAsync(id); return Ok(ApiResponse<object>.Ok(null)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/start")]
    public async Task<IActionResult> Start(int id)
    {
        try { return Ok(ApiResponse<MeetingDetailDto>.Ok(await _svc.StartAsync(id))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] SaveMinutesRequest req)
    {
        try { return Ok(ApiResponse<MeetingDetailDto>.Ok(await _svc.CompleteAsync(id, req))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try { return Ok(ApiResponse<MeetingDetailDto>.Ok(await _svc.CancelAsync(id))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPost("{id:int}/actions/{actionId:int}/complete")]
    public async Task<IActionResult> CompleteAction(int id, int actionId)
    {
        try { return Ok(ApiResponse<MeetingActionItemDto>.Ok(await _svc.CompleteActionAsync(id, actionId))); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    private int? GetUserId() =>
        int.TryParse(_cu.UserId, out var id) ? id : null;

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
        is { Count: > 0 } errs ? errs : ["طلب غير صالح."];
}
