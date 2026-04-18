using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/auth/uaepass")]
[Produces("application/json")]
public class UaePassController : ControllerBase
{
    private readonly IUaePassService     _uaePassService;
    private readonly ITenantConfigLoader _tenantLoader;
    private readonly IJwtService         _jwtService;
    private readonly ISessionService     _sessionService;
    private readonly TenantContext       _tenant;

    public UaePassController(
        IUaePassService     uaePassService,
        ITenantConfigLoader tenantLoader,
        IJwtService         jwtService,
        ISessionService     sessionService,
        TenantContext       tenant)
    {
        _uaePassService = uaePassService;
        _tenantLoader   = tenantLoader;
        _jwtService     = jwtService;
        _sessionService = sessionService;
        _tenant         = tenant;
    }

    /// <summary>
    /// Step 1 — Returns the UAE Pass authorization URL.
    /// Frontend redirects the browser to this URL (window.location.href).
    /// State encodes the tenant key so Step 2 can resolve it without a header.
    /// </summary>
    [HttpGet("redirect")]
    [ProducesResponseType(typeof(ApiResponse<UaePassRedirectResponse>), StatusCodes.Status200OK)]
    public IActionResult GetRedirectUrl([FromQuery] string language = "ar")
    {
        // Embed tenant key in state so exchange endpoint can resolve it
        var state = $"{_tenant.TenantKey}:{Guid.NewGuid():N}";
        var url   = _uaePassService.BuildAuthorizationUrl(state, language);

        return Ok(ApiResponse<UaePassRedirectResponse>.Ok(new UaePassRedirectResponse
        {
            RedirectUrl = url,
            State       = state,
        }));
    }

    /// <summary>
    /// Step 2 — Called by the React frontend after UAE Pass redirects back with ?code=...
    /// Exchanges the authorization code for a UAE Pass user profile, creates an IGMS session,
    /// and returns a JWT token ready to store in sessionStorage.
    ///
    /// This is a regular Axios call from the frontend (has X-Tenant-Key header).
    /// </summary>
    [HttpPost("exchange")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Exchange([FromBody] UaePassExchangeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(ApiResponse<object>.Fail("رمز التفويض مفقود."));

        // Exchange code for UAE Pass user info (server → UAE Pass, no CORS issue)
        var result = await _uaePassService.ExchangeCodeAsync(request.Code);
        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<object>.Unauthorized(result.Error!));

        var user  = result.Value!;
        var roles = new List<string> { "User" };

        var token = _jwtService.GenerateToken(
            userId:      user.Sub,
            username:    user.EmiratesId,
            roles:       roles,
            permissions: [],
            tenantKey:   _tenant.TenantKey,
            language:    "ar");

        var session = new SessionData
        {
            UserId       = user.Sub,
            Username     = user.EmiratesId,
            FullNameAr   = user.FullNameAr,
            FullNameEn   = user.FullNameEn,
            TenantKey    = _tenant.TenantKey,
            Roles        = roles,
            Language     = "ar",
            AuthProvider = "UaePass",
            ExpiresAt    = _jwtService.GetTokenExpiry(),
        };

        var sessionId = await _sessionService.CreateSessionAsync(session);

        return Ok(ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            Token      = token,
            SessionId  = sessionId,
            FullNameAr = user.FullNameAr,
            FullNameEn = user.FullNameEn,
            Roles      = roles,
            Language   = "ar",
        }));
    }
}

public class UaePassExchangeRequest
{
    public string Code  { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
