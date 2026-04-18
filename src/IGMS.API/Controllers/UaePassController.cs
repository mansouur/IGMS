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
    private readonly IUaePassService _uaePassService;
    private readonly IAuthStrategy _localStrategy;
    private readonly IJwtService _jwtService;
    private readonly ISessionService _sessionService;
    private readonly TenantContext _tenant;
    private readonly IConfiguration _configuration;

    public UaePassController(
        IUaePassService uaePassService,
        IEnumerable<IAuthStrategy> strategies,
        IJwtService jwtService,
        ISessionService sessionService,
        TenantContext tenant,
        IConfiguration configuration)
    {
        _uaePassService = uaePassService;
        _localStrategy = strategies.First(s => s.ProviderName == "Local");
        _jwtService = jwtService;
        _sessionService = sessionService;
        _tenant = tenant;
        _configuration = configuration;
    }

    /// <summary>
    /// Returns the UAE Pass authorization URL.
    /// React app redirects the user to this URL to start the OAuth flow.
    /// </summary>
    [HttpGet("redirect")]
    [ProducesResponseType(typeof(ApiResponse<UaePassRedirectResponse>), StatusCodes.Status200OK)]
    public IActionResult GetRedirectUrl([FromQuery] string language = "ar")
    {
        var state = Guid.NewGuid().ToString("N");
        var url = _uaePassService.BuildAuthorizationUrl(state, language);

        return Ok(ApiResponse<UaePassRedirectResponse>.Ok(new UaePassRedirectResponse
        {
            RedirectUrl = url,
            State = state
        }));
    }

    /// <summary>
    /// UAE Pass callback endpoint.
    /// UAE Pass redirects here after the user authenticates.
    /// Exchanges the code for a user profile, creates a session, and redirects to the React app.
    /// </summary>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromQuery] string? error = null)
    {
        var frontendUrl = _configuration["UaePass:FrontendCallbackUrl"]
            ?? "http://localhost:5173/auth/callback";

        if (!string.IsNullOrEmpty(error))
            return Redirect($"{frontendUrl}?error={Uri.EscapeDataString(error)}");

        var result = await _uaePassService.ExchangeCodeAsync(code);

        if (!result.IsSuccess)
            return Redirect($"{frontendUrl}?error={Uri.EscapeDataString(result.Error!)}");

        var user = result.Value!;
        var roles = new List<string> { "User" }; // TODO (Phase 1): map UAE Pass roles
        var token = _jwtService.GenerateToken(
            userId: user.Sub,
            username: user.EmiratesId,
            roles: roles,
            permissions: [],  // TODO (Phase 2): load permissions from DB by UAE Pass sub
            tenantKey: _tenant.TenantKey,
            language: "ar");

        var session = new SessionData
        {
            UserId = user.Sub,
            Username = user.EmiratesId,
            FullNameAr = user.FullNameAr,
            FullNameEn = user.FullNameEn,
            TenantKey = _tenant.TenantKey,
            Roles = roles,
            Language = "ar",
            AuthProvider = "UaePass",
            ExpiresAt = _jwtService.GetTokenExpiry()
        };

        var sessionId = await _sessionService.CreateSessionAsync(session);

        // Redirect React app with token + sessionId in URL fragment (not query string – more secure)
        return Redirect($"{frontendUrl}#token={token}&sessionId={sessionId}");
    }
}
