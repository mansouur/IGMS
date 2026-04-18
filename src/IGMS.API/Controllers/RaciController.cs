using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/raci")]
[Produces("application/json")]
[Authorize]
public class RaciController : ControllerBase
{
    private readonly IRaciService _raciService;
    private readonly ICurrentUserService _currentUser;

    public RaciController(IRaciService raciService, ICurrentUserService currentUser)
    {
        _raciService  = raciService;
        _currentUser  = currentUser;
    }

    /// <summary>
    /// قائمة مصفوفات RACI مع دعم البحث والتصفية والترقيم.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RaciMatrixListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page          = 1,
        [FromQuery] int pageSize      = 10,
        [FromQuery] string? search    = null,
        [FromQuery] RaciStatus? status = null,
        [FromQuery] int? departmentId = null)
    {
        var query = new RaciMatrixQuery
        {
            Page         = page,
            PageSize     = Math.Clamp(pageSize, 1, 100),
            Search       = search,
            Status       = status,
            DepartmentId = departmentId
        };

        var result = await _raciService.GetPagedAsync(query);
        return Ok(ApiResponse<PagedResult<RaciMatrixListDto>>.Ok(result.Value!));
    }

    /// <summary>
    /// تفاصيل مصفوفة RACI مع كامل الأنشطة والمشاركين.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RaciMatrixDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _raciService.GetByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.NotFound(result.Error!));

        return Ok(ApiResponse<RaciMatrixDetailDto>.Ok(result.Value!));
    }

    /// <summary>
    /// إنشاء مصفوفة RACI جديدة.
    /// يتطلب صلاحية RACI.CREATE.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RaciMatrixDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRaciMatrixRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

        var result = await _raciService.CreateAsync(request, _currentUser.Username);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<RaciMatrixDetailDto>.Created(result.Value!));
    }

    /// <summary>
    /// تعديل مصفوفة RACI (Draft أو UnderReview فقط).
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RaciMatrixDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRaciMatrixRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

        request.Id = id;
        var result = await _raciService.UpdateAsync(request, _currentUser.Username);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<RaciMatrixDetailDto>.Ok(result.Value!));
    }

    /// <summary>
    /// حذف مصفوفة RACI (Soft Delete – لا يُطبَّق على المعتمدة).
    /// يتطلب صلاحية RACI.DELETE.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _raciService.DeleteAsync(id, _currentUser.Username);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<object>.Ok(null, "تم حذف المصفوفة بنجاح."));
    }

    /// <summary>
    /// إرسال المصفوفة للمراجعة (Draft → UnderReview).
    /// </summary>
    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(int id)
    {
        var result = await _raciService.SubmitForReviewAsync(id, _currentUser.Username);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<object>.Ok(null, "تم إرسال المصفوفة للمراجعة."));
    }

    /// <summary>
    /// اعتماد مصفوفة RACI (UnderReview → Approved).
    /// يتطلب صلاحية RACI.APPROVE.
    /// </summary>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(typeof(ApiResponse<RaciMatrixDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(int id)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId))
            return Unauthorized(ApiResponse<object>.Unauthorized("معرّف المستخدم غير صالح."));

        var result = await _raciService.ApproveAsync(id, _currentUser.Username, userId);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<RaciMatrixDetailDto>.Ok(result.Value!));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private List<string> GetModelErrors() =>
        ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
}
