using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/vendors")]
[Produces("application/json")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _svc;

    public VendorsController(IVendorService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] VendorQuery query)
    {
        var result = await _svc.GetPagedAsync(query);
        return Ok(ApiResponse<PagedResult<VendorListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _svc.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse<object>.NotFound("المورد غير موجود."));
        return Ok(ApiResponse<VendorDetailDto>.Ok(dto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveVendorRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        try
        {
            var dto = await _svc.CreateAsync(req);
            return Created($"/api/v1/vendors/{dto.Id}", ApiResponse<VendorDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveVendorRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail(GetErrors()));
        try
        {
            var dto = await _svc.UpdateAsync(id, req);
            return Ok(ApiResponse<VendorDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _svc.DeleteAsync(id); return Ok(ApiResponse<object>.Ok(null)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    [HttpPut("{id:int}/risk")]
    public async Task<IActionResult> AssessRisk(int id, [FromBody] AssessVendorRiskRequest req)
    {
        try
        {
            var dto = await _svc.AssessRiskAsync(id, req);
            return Ok(ApiResponse<VendorDetailDto>.Ok(dto));
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(ex.Message)); }
    }

    private List<string> GetErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
        is { Count: > 0 } errs ? errs : ["طلب غير صالح."];
}
