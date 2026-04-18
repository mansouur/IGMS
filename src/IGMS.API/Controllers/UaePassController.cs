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
    private readonly IUaePassService       _uaePassService;
    private readonly ITenantConfigLoader   _tenantLoader;
    private readonly IJwtService           _jwtService;
    private readonly ISessionService       _sessionService;
    private readonly TenantContext         _tenant;          // populated for redirect endpoint
    private readonly IConfiguration       _configuration;

    public UaePassController(
        IUaePassService     uaePassService,
        ITenantConfigLoader tenantLoader,
        IJwtService         jwtService,
        ISessionService     sessionService,
        TenantContext       tenant,
        IConfiguration      configuration)
    {
        _uaePassService = uaePassService;
        _tenantLoader   = tenantLoader;
        _jwtService     = jwtService;
        _sessionService = sessionService;
        _tenant         = tenant;
        _configuration  = configuration;
    }

    /// <summary>
    /// Returns the UAE Pass authorization URL.
    /// The tenant key is embedded in the state so the callback can resolve it
    /// without relying on the X-Tenant-Key header (which a browser redirect can't send).
    /// </summary>
    [HttpGet("redirect")]
    [ProducesResponseType(typeof(ApiResponse<UaePassRedirectResponse>), StatusCodes.Status200OK)]
    public IActionResult GetRedirectUrl([FromQuery] string language = "ar")
    {
        // state = "{tenantKey}:{random}" — tenant travels with the OAuth state
        var state = $"{_tenant.TenantKey}:{Guid.NewGuid():N}";
        var url   = _uaePassService.BuildAuthorizationUrl(state, language);

        return Ok(ApiResponse<UaePassRedirectResponse>.Ok(new UaePassRedirectResponse
        {
            RedirectUrl = url,
            State       = state,
        }));
    }

    /// <summary>
    /// UAE Pass callback — this endpoint is called by a browser redirect from UAE Pass,
    /// so no X-Tenant-Key header is present. The tenant is recovered from the state parameter.
    /// </summary>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string  code,
        [FromQuery] string  state,
        [FromQuery] string? error = null)
    {
        var frontendUrl = _configuration["UaePass:FrontendCallbackUrl"]
            ?? "http://localhost:5173/auth/callback";

        if (!string.IsNullOrEmpty(error))
            return Redirect($"{frontendUrl}?error={Uri.EscapeDataString(error)}");

        // ── Resolve tenant from state ─────────────────────────────────────────
        // state format: "{tenantKey}:{randomGuid}"
        var tenantKey = state?.Split(':')[0];
        if (string.IsNullOrWhiteSpace(tenantKey))
            return Redirect($"{frontendUrl}?error={Uri.EscapeDataString("حالة الطلب غير صالحة.")}");

        var tenant = await _tenantLoader.LoadAsync(tenantKey);
        if (tenant is null)
            return Redirect($"{frontendUrl}?error={Uri.EscapeDataString("Tenant not found.")}");

        // ── Exchange code for UAE Pass user info ──────────────────────────────
        var result = await _uaePassService.ExchangeCodeAsync(code);
        if (!result.IsSuccess)
            return Redirect($"{frontendUrl}?error={Uri.EscapeDataString(result.Error!)}");

        var user  = result.Value!;
        var roles = new List<string> { "User" };

        var token = _jwtService.GenerateToken(
            userId:      user.Sub,
            username:    user.EmiratesId,
            roles:       roles,
            permissions: [],
            tenantKey:   tenant.TenantKey,
            language:    "ar");

        var session = new SessionData
        {
            UserId       = user.Sub,
            Username     = user.EmiratesId,
            FullNameAr   = user.FullNameAr,
            FullNameEn   = user.FullNameEn,
            TenantKey    = tenant.TenantKey,
            Roles        = roles,
            Language     = "ar",
            AuthProvider = "UaePass",
            ExpiresAt    = _jwtService.GetTokenExpiry(),
        };

        var sessionId = await _sessionService.CreateSessionAsync(session);

        return Redirect(
            $"{frontendUrl}#token={token}&sessionId={sessionId}" +
            $"&fullNameAr={Uri.EscapeDataString(user.FullNameAr)}" +
            $"&fullNameEn={Uri.EscapeDataString(user.FullNameEn)}");
    }
}
