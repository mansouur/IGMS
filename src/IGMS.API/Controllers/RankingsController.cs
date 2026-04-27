using IGMS.Application.Common.Interfaces;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/rankings")]
[Produces("application/json")]
[Authorize]
public class RankingsController : ControllerBase
{
    private readonly IRankingService     _svc;
    private readonly ICurrentUserService _cu;

    public RankingsController(IRankingService svc, ICurrentUserService cu)
    {
        _svc = svc;
        _cu  = cu;
    }

    private int  CurrentUserId => int.TryParse(_cu.UserId, out var id) ? id : 0;
    private bool IsAdmin       => _cu.Roles.Contains("ADMIN");
    private bool IsDeptManager => IsAdmin || _cu.Roles.Contains("MANAGER");

    /// <summary>ترتيب الأقسام — الأدمن يرى الجميع، غيره يرى قسمه فقط.</summary>
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartmentRankings()
    {
        var result = await _svc.GetDepartmentRankingsAsync(CurrentUserId, IsAdmin);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>ترتيب الموظفين — نطاق العرض يعتمد على الصلاحية.</summary>
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployeeRankings()
    {
        var result = await _svc.GetEmployeeRankingsAsync(CurrentUserId, IsAdmin, IsDeptManager);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>درجتي الشخصية بالتفصيل — لكل مستخدم مسجّل.</summary>
    [HttpGet("my-score")]
    public async Task<IActionResult> GetMyScore()
    {
        var result = await _svc.GetMyScoreAsync(CurrentUserId);
        return Ok(ApiResponse<object>.Ok(result));
    }
}
