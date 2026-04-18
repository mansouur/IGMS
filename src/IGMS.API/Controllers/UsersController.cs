using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IUserService userService, ICurrentUserService currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    /// <summary>قائمة المستخدمين مع البحث والتصفية والترقيم.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int     page         = 1,
        [FromQuery] int     pageSize     = 20,
        [FromQuery] string? search       = null,
        [FromQuery] int?    departmentId = null,
        [FromQuery] bool?   isActive     = null)
    {
        var query = new UserQuery
        {
            Page         = page,
            PageSize     = Math.Clamp(pageSize, 1, 100),
            Search       = search,
            DepartmentId = departmentId,
            IsActive     = isActive,
        };

        var result = await _userService.GetPagedAsync(query);
        return Ok(ApiResponse<PagedResult<UserListDto>>.Ok(result.Value!));
    }

    /// <summary>قائمة مختصرة (Id + الاسم) لاستخدامها في UserIdPicker.</summary>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(ApiResponse<List<UserListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLookup()
    {
        var result = await _userService.GetLookupAsync();
        return Ok(ApiResponse<List<UserListDto>>.Ok(result.Value!));
    }

    /// <summary>تفاصيل مستخدم محدد.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.NotFound(result.Error!));

        return Ok(ApiResponse<UserDetailDto>.Ok(result.Value!));
    }

    /// <summary>إنشاء مستخدم جديد.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

        var result = await _userService.CreateAsync(request, _currentUser.Username);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<UserDetailDto>.Created(result.Value!));
    }

    /// <summary>تعديل بيانات مستخدم.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

        request.Id = id;
        var result = await _userService.UpdateAsync(request, _currentUser.Username);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));

        return Ok(ApiResponse<UserDetailDto>.Ok(result.Value!));
    }

    /// <summary>حذف مستخدم (Soft Delete).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id, _currentUser.Username);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.NotFound(result.Error!));

        return Ok(ApiResponse<object>.Ok(null, "تم حذف المستخدم بنجاح."));
    }

    /// <summary>تفعيل أو تعطيل مستخدم.</summary>
    [HttpPatch("{id:int}/active")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetActiveRequest request)
    {
        var result = await _userService.SetActiveAsync(id, request.IsActive, _currentUser.Username);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.NotFound(result.Error!));

        var msg = request.IsActive ? "تم تفعيل المستخدم." : "تم تعطيل المستخدم.";
        return Ok(ApiResponse<object>.Ok(null, msg));
    }

    /// <summary>تصدير المستخدمين إلى Excel.</summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] UserQuery q)
    {
        var bytes = await _userService.ExportAsync(q);
        var fileName = $"users_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>تغيير كلمة مرور المستخدم الحالي.</summary>
    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!int.TryParse(_currentUser.UserId, out var uid))
            return BadRequest(ApiResponse<object>.Fail("معرّف المستخدم غير صالح."));

        var result = await _userService.ChangePasswordAsync(
            uid, request.CurrentPassword, request.NewPassword);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error!));
        return Ok(ApiResponse<object>.Ok(null, "تم تغيير كلمة المرور."));
    }

    private List<string> GetModelErrors() =>
        ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
