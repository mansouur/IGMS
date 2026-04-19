using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Infrastructure.Auth.Strategies;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/auth/uaepass")]
[Produces("application/json")]
public class UaePassController : ControllerBase
{
    private readonly IUaePassService _uaePassService;

    public UaePassController(IUaePassService uaePassService)
    {
        _uaePassService = uaePassService;
    }

    /// <summary>
    /// Step 1 — Browser navigates here directly (window.location.href).
    /// Builds the UAE Pass authorization URL and returns 302 → UAE Pass login page.
    /// Tenant key is passed as a query param (no X-Tenant-Key header in browser navigation).
    /// </summary>
    [HttpGet("redirect")]
    public IActionResult Redirect(
        [FromQuery] string language = "ar",
        [FromQuery] string tenant   = "")
    {
        var state = $"{tenant}:{Guid.NewGuid():N}";
        var url   = _uaePassService.BuildAuthorizationUrl(state, language);
        return base.Redirect(url);
    }

    /// <summary>
    /// Ends the UAE Pass SSO session and redirects back to the IGMS login page.
    /// Browser navigates here directly after IGMS session is cleared.
    /// </summary>
    [HttpGet("logout")]
    public IActionResult UaePassLogout([FromQuery] string redirectUri = "")
    {
        var postLogout = string.IsNullOrWhiteSpace(redirectUri)
            ? "http://localhost:5173/login"
            : redirectUri;
        return base.Redirect(_uaePassService.BuildLogoutUrl(postLogout));
    }

    /// <summary>
    /// Step 2 — Called by React after UAE Pass redirects back with ?code=...
    /// Exchanges the code for a UAE Pass profile, matches the user by Emirates ID,
    /// and returns a full IGMS JWT with the user's actual roles and permissions.
    /// </summary>
    [HttpPost("exchange")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Exchange(
        [FromBody]    UaePassExchangeRequest    request,
        [FromServices] IEnumerable<IAuthStrategy> strategies,
        [FromServices] TenantContext              tenant,
        [FromServices] TenantDbContext            db)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(ApiResponse<object>.Fail("رمز التفويض مفقود."));

        // ── 1. Exchange code for UAE Pass user info ───────────────────────────
        var result = await _uaePassService.ExchangeCodeAsync(request.Code);
        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<object>.Unauthorized(result.Error!));

        var uaeUser = result.Value!;

        if (string.IsNullOrWhiteSpace(uaeUser.EmiratesId))
            return Unauthorized(ApiResponse<object>.Unauthorized(
                "تعذّر استرجاع رقم الهوية الإماراتية من UAE Pass."));

        // ── 2. Find matching UserProfile by Emirates ID ───────────────────────
        var user = await db.UserProfiles
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.Department)
            .Where(u => u.EmiratesId == uaeUser.EmiratesId && u.IsActive && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (user is null)
            return Unauthorized(ApiResponse<object>.Unauthorized(
                "ليس لديك صلاحية الوصول إلى النظام. هذا النظام خاص بموظفي الوزارة المسجّلين."));

        // ── 3. Update last login ──────────────────────────────────────────────
        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // ── 4. Build JWT + session with actual roles & permissions ────────────
        var localStrategy          = strategies.OfType<LocalAuthStrategy>().First();
        var (roles, perms)         = LocalAuthStrategy.CollectRolesAndPermissions(user);
        var loginResult            = await localStrategy.BuildResponseAsync(user, roles, perms, tenant, "ar");
        var response               = loginResult.Value!;
        response.AuthProvider      = "UaePass";

        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }
}

public class UaePassExchangeRequest
{
    public string Code  { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
