using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;
using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure.Auth.Strategies;

/// <summary>
/// Active Directory authentication via LDAP.
///
/// Flow:
///   1. Validates username + password against the AD domain (LDAP bind).
///   2. Looks up the matching UserProfile in IGMS by username.
///   3. Builds JWT + session with IGMS roles and permissions (not AD groups).
///
/// Activation (tenant config only — no code changes needed):
///   authentication.mode = "AD"    → all logins go through AD
///   authentication.mode = "Mixed" → regular form uses Local, AD via /auth/login/ad
///
/// Username format expected: firstname.lastname  (e.g. mansour.alsharif)
/// Must match the Username field in UserProfiles exactly.
/// </summary>
public class AdAuthStrategy : IAuthStrategy
{
    public string ProviderName => "AD";

    private readonly TenantDbContext            _db;
    private readonly IJwtService                _jwtService;
    private readonly ISessionService            _sessionService;
    private readonly ILogger<AdAuthStrategy>    _logger;

    public AdAuthStrategy(
        TenantDbContext          db,
        IJwtService              jwtService,
        ISessionService          sessionService,
        ILogger<AdAuthStrategy>  logger)
    {
        _db             = db;
        _jwtService     = jwtService;
        _sessionService = sessionService;
        _logger         = logger;
    }

    [SupportedOSPlatform("windows")]
    public async Task<Result<LoginResponse>> AuthenticateAsync(LoginRequest request, TenantContext tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant.Authentication.Domain))
            return Result<LoginResponse>.Failure("لم يتم تهيئة نطاق Active Directory لهذا المستأجر.");

        // ── 1. Validate credentials against AD ───────────────────────────────
        var adValid = ValidateAdCredentials(request.Username, request.Password, tenant.Authentication.Domain);
        if (!adValid)
        {
            _logger.LogWarning("AD login failed – bad credentials: {Username}@{Domain}",
                request.Username, tenant.Authentication.Domain);
            return Result<LoginResponse>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة.");
        }

        // ── 2. Find matching UserProfile in IGMS by username ─────────────────
        var user = await _db.UserProfiles
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.Department)
            .Where(u => u.Username == request.Username && u.IsActive && !u.IsDeleted)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            _logger.LogWarning("AD auth OK but no IGMS profile found: {Username}", request.Username);
            return Result<LoginResponse>.Failure(
                "تم التحقق من هويتك بنجاح، لكن ليس لديك حساب مفعّل في النظام. تواصل مع مدير النظام.");
        }

        // ── 3. Update last login ──────────────────────────────────────────────
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // ── 4. Build JWT with IGMS roles & permissions ────────────────────────
        var (roles, perms) = LocalAuthStrategy.CollectRolesAndPermissions(user);
        return await BuildResponseAsync(user, roles, perms, tenant, request.Language);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    [SupportedOSPlatform("windows")]
    private bool ValidateAdCredentials(string username, string password, string domain)
    {
        try
        {
            using var context = new PrincipalContext(ContextType.Domain, domain);
            return context.ValidateCredentials(username, password);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AD connection error – domain: {Domain}", domain);
            return false;
        }
    }

    private async Task<Result<LoginResponse>> BuildResponseAsync(
        Domain.Entities.UserProfile user,
        List<string> roles,
        List<string> permissions,
        TenantContext tenant,
        string language)
    {
        var token  = _jwtService.GenerateToken(
            userId:      user.Id.ToString(),
            username:    user.Username,
            roles:       roles,
            permissions: permissions,
            tenantKey:   tenant.TenantKey,
            language:    language);

        var expiry = _jwtService.GetTokenExpiry();

        var session = new SessionData
        {
            UserId       = user.Id.ToString(),
            Username     = user.Username,
            FullNameAr   = user.FullNameAr,
            FullNameEn   = user.FullNameEn,
            TenantKey    = tenant.TenantKey,
            Roles        = roles,
            Language     = language,
            AuthProvider = ProviderName,
            ExpiresAt    = expiry,
        };

        var sessionId = await _sessionService.CreateSessionAsync(session);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            Token        = token,
            SessionId    = sessionId,
            AuthProvider = ProviderName,
            Username     = user.Username,
            FullNameAr   = user.FullNameAr,
            FullNameEn   = user.FullNameEn,
            Roles        = roles,
            ExpiresAt    = expiry,
            Language     = language,
            Organization = tenant.Organization,
        });
    }
}
