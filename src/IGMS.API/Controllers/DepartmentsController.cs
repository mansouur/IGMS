using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/departments")]
[Produces("application/json")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _deptService;
    private readonly ICurrentUserService _currentUser;

    public DepartmentsController(IDepartmentService deptService, ICurrentUserService currentUser)
    {
        _deptService = deptService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// قائمة الأقسام مع البحث والتصفية والترقيم.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DepartmentListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20,
        [FromQuery] string? search   = null,
        [FromQuery] int?    parentId = null,
        [FromQuery] bool?   isActive = null)
    {
        var query = new DepartmentQuery
        {
            Page     = page,
            PageSize = Math.Clamp(pageSize, 1, 100),
            Search   = search,
            ParentId = parentId,
            IsActive = isActive,
        };

        var result = await _deptService.GetPagedAsync(query);
        return Ok(ApiResponse<PagedResult<DepartmentListDto>>.Ok(result.Value!));
    }

    /// <summary>
    /// الهيكل التنظيمي الشجري (للعرض في القوائم المنسدلة والشجرة).
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentTreeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree()
    {
        var result = await _deptService.GetTreeAsync();
        return Ok(ApiResponse<List<DepartmentTreeDto>>.Ok(result.Value!));
    }

    /// <summary>
    /// تفاصيل قسم محدد مع الأقسام الفرعية.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _deptService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.NotFound(result.Error!));

        return Ok(ApiResponse<DepartmentDetailDto>.Ok(result.Value!));
    }

    /// <summary>
    /// إنشاء قسم جديد.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

        var result = await _deptService.CreateAsync(request, _currentUser.Username);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<DepartmentDetailDto>.Created(result.Value!));
    }

    /// <summary>
    /// تعديل بيانات قسم.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

        request.Id = id;
        var result = await _deptService.UpdateAsync(request, _currentUser.Username);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<DepartmentDetailDto>.Ok(result.Value!));
    }

    /// <summary>
    /// حذف قسم (Soft Delete). يُرفض إذا كان يحتوي على أقسام فرعية أو موظفين.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _deptService.DeleteAsync(id, _currentUser.Username);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<object>.Ok(null, "تم حذف القسم بنجاح."));
    }

    /// <summary>
    /// تفعيل أو تعطيل قسم.
    /// </summary>
    [HttpPatch("{id:int}/active")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetActiveRequest request)
    {
        var result = await _deptService.SetActiveAsync(id, request.IsActive, _currentUser.Username);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.NotFound(result.Error!));

        var msg = request.IsActive ? "تم تفعيل القسم." : "تم تعطيل القسم.";
        return Ok(ApiResponse<object>.Ok(null, msg));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private List<string> GetModelErrors() =>
        ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
}

/// <summary>Simple body for PATCH /active</summary>
public record SetActiveRequest(bool IsActive);
