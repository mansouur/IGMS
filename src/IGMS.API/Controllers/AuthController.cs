using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Infrastructure.Auth.Strategies;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IEnumerable<IAuthStrategy> _strategies;
    private readonly ISessionService             _sessionService;
    private readonly TenantContext               _tenant;
    private readonly TenantDbContext             _db;
    private readonly IOtpService                 _otp;
    private readonly ICurrentUserService         _cu;

    public AuthController(
        IEnumerable<IAuthStrategy> strategies,
        ISessionService             sessionService,
        TenantContext               tenant,
        TenantDbContext             db,
        IOtpService                 otp,
        ICurrentUserService         cu)
    {
        _strategies     = strategies;
        _sessionService = sessionService;
        _tenant         = tenant;
        _db             = db;
        _otp            = otp;
        _cu             = cu;
    }

    /// <summary>
    /// Local or AD login. Provider is selected based on tenant config.
    /// Returns JWT token + session ID, OR RequiresOtp = true when 2FA is enabled.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<object>.Fail(errors));
        }

        // Select strategy based on tenant auth mode
        var providerName = _tenant.Authentication.Mode switch
        {
            "AD"    => "AD",
            "Mixed" => "Local",
            _       => "Local"
        };

        var strategy = _strategies.FirstOrDefault(s => s.ProviderName == providerName);
        if (strategy is null)
            return StatusCode(500, ApiResponse<object>.Fail("Auth strategy not configured."));

        var result = await strategy.AuthenticateAsync(request, _tenant);

        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<LoginResponse>.Unauthorized(result.Error!));

        return Ok(ApiResponse<LoginResponse>.Ok(result.Value!));
    }

    /// <summary>
    /// AD-specific login. Used when tenant mode is "Mixed" and user selects AD.
    /// </summary>
    [HttpPost("login/ad")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginWithAd([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request."));

        var strategy = _strategies.FirstOrDefault(s => s.ProviderName == "AD");
        if (strategy is null)
            return BadRequest(ApiResponse<object>.Fail("AD login is not enabled for this tenant."));

        var result = await strategy.AuthenticateAsync(request, _tenant);

        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<LoginResponse>.Unauthorized(result.Error!));

        return Ok(ApiResponse<LoginResponse>.Ok(result.Value!));
    }

    /// <summary>
    /// Completes Two-Factor Authentication by validating the emailed OTP.
    /// Returns full JWT + session on success.
    /// </summary>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("بيانات غير صحيحة."));

        var isValid = await _otp.ValidateAndConsumeAsync(request.UserId, request.Otp);
        if (!isValid)
            return Unauthorized(ApiResponse<object>.Unauthorized("رمز التحقق غير صحيح أو منتهي الصلاحية."));

        // Load user with roles and permissions to build the full JWT
        var user = await _db.UserProfiles
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.Department)
            .Where(u => u.Id == request.UserId && u.IsActive)
            .FirstOrDefaultAsync();

        if (user is null)
            return Unauthorized(ApiResponse<object>.Unauthorized("المستخدم غير موجود."));

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var localStrategy = _strategies.OfType<LocalAuthStrategy>().FirstOrDefault();
        if (localStrategy is null)
            return StatusCode(500, ApiResponse<object>.Fail("Auth strategy error."));

        var (roles, perms) = LocalAuthStrategy.CollectRolesAndPermissions(user);
        var loginResult    = await localStrategy.BuildResponseAsync(user, roles, perms, _tenant, request.Language);

        return Ok(ApiResponse<LoginResponse>.Ok(loginResult.Value!));
    }

    /// <summary>
    /// Toggles Two-Factor Authentication for the currently authenticated user.
    /// Requires current password for security.
    /// </summary>
    [HttpPut("2fa")]
    [Authorize]
    public async Task<IActionResult> Toggle2Fa([FromBody] Toggle2FaRequest request)
    {
        var userId = int.Parse(_cu.UserId ?? "0");
        var user   = await _db.UserProfiles.FindAsync(userId);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            return NotFound(ApiResponse<object>.NotFound("المستخدم غير موجود."));

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(ApiResponse<object>.Unauthorized("كلمة المرور غير صحيحة."));

        user.TwoFactorEnabled = request.Enabled;
        await _db.SaveChangesAsync();

        var msg = request.Enabled
            ? "تم تفعيل المصادقة الثنائية بنجاح."
            : "تم إلغاء المصادقة الثنائية.";

        return Ok(ApiResponse<object>.Ok(new { twoFactorEnabled = request.Enabled }, msg));
    }

    /// <summary>
    /// Logout: revokes the session from cache.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await _sessionService.RevokeSessionAsync(request.SessionId);
        return Ok(ApiResponse<object>.Ok(null, "تم تسجيل الخروج بنجاح."));
    }

    /// <summary>
    /// Returns the current user's profile info including 2FA status.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(_cu.UserId ?? "0");
        var user   = await _db.UserProfiles
            .Where(u => u.Id == userId && u.IsActive)
            .Select(u => new { u.Id, u.Username, u.FullNameAr, u.FullNameEn, u.Email, u.TwoFactorEnabled })
            .FirstOrDefaultAsync();

        if (user is null)
            return NotFound(ApiResponse<object>.NotFound("المستخدم غير موجود."));

        return Ok(ApiResponse<object>.Ok(user));
    }

    /// <summary>
    /// Returns available login methods for this tenant.
    /// </summary>
    [HttpGet("methods")]
    public IActionResult GetAuthMethods()
    {
        var mode    = _tenant.Authentication.Mode;
        var methods = new AuthMethodsResponse
        {
            Local   = mode is "Local" or "Mixed",
            Ad      = mode is "AD" or "Mixed",
            UaePass = mode is "UaePass" or "Mixed"
        };

        return Ok(ApiResponse<AuthMethodsResponse>.Ok(methods));
    }
}

public class LogoutRequest
{
    public string SessionId { get; set; } = string.Empty;
}

public class AuthMethodsResponse
{
    public bool Local   { get; set; }
    public bool Ad      { get; set; }
    public bool UaePass { get; set; }
}
