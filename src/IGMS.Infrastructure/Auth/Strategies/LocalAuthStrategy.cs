using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Infrastructure.Persistence;
using IGMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure.Auth.Strategies;

/// <summary>
/// Local database authentication strategy.
/// Validates credentials against UserProfiles table using BCrypt.
/// Loads all user permissions via Role → RolePermission → Permission chain.
/// When the user has TwoFactorEnabled: returns RequiresOtp = true instead of a full JWT.
/// The client must then call POST /api/v1/auth/verify-otp to complete authentication.
/// </summary>
public class LocalAuthStrategy : IAuthStrategy
{
    public string ProviderName => "Local";

    private readonly TenantDbContext      _db;
    private readonly IJwtService          _jwtService;
    private readonly ISessionService      _sessionService;
    private readonly IOtpService          _otpService;
    private readonly INotificationService _notifySvc;
    private readonly ILogger<LocalAuthStrategy> _logger;

    public LocalAuthStrategy(
        TenantDbContext      db,
        IJwtService          jwtService,
        ISessionService      sessionService,
        IOtpService          otpService,
        INotificationService notifySvc,
        ILogger<LocalAuthStrategy> logger)
    {
        _db             = db;
        _jwtService     = jwtService;
        _sessionService = sessionService;
        _otpService     = otpService;
        _notifySvc      = notifySvc;
        _logger         = logger;
    }

    public async Task<Result<LoginResponse>> AuthenticateAsync(LoginRequest request, TenantContext tenant)
    {
        var user = await _db.UserProfiles
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.Department)
            .Where(u => u.Username == request.Username && u.IsActive)
            .FirstOrDefaultAsync();

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
        {
            _logger.LogWarning("Login failed – user not found: {Username}", request.Username);
            return Result<LoginResponse>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed – wrong password: {Username}", request.Username);
            return Result<LoginResponse>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة.");
        }

        // ── Two-Factor Authentication check ───────────────────────────────────
        if (user.TwoFactorEnabled)
        {
            _logger.LogInformation("2FA required for user {Username}. Sending OTP.", request.Username);

            var otp = await _otpService.GenerateAndStoreAsync(user.Id);

            try
            {
                await _notifySvc.SendOtpEmailAsync(
                    recipientEmail:   user.Email,
                    recipientNameAr:  user.FullNameAr,
                    otp:              otp,
                    organizationName: tenant.Organization.NameAr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}.", user.Email);
                // Don't fail the login — return pending state anyway (OTP stored in cache)
            }

            return Result<LoginResponse>.Success(new LoginResponse
            {
                RequiresOtp   = true,
                PendingUserId = user.Id,
                Language      = request.Language,
            });
        }

        // ── No 2FA: complete login normally ───────────────────────────────────
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var (roles, perms) = CollectRolesAndPermissions(user);
        return await BuildResponseAsync(user, roles, perms, tenant, request.Language);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    public static (List<string> roles, List<string> permissions)
        CollectRolesAndPermissions(Domain.Entities.UserProfile user)
    {
        var active = user.UserRoles.Where(ur => ur.IsActive).ToList();
        var roles  = active.Select(ur => ur.Role.Code).Distinct().ToList();
        var perms  = active
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();
        return (roles, perms);
    }

    public async Task<Result<LoginResponse>> BuildResponseAsync(
        Domain.Entities.UserProfile user,
        List<string> roles,
        List<string> permissions,
        TenantContext tenant,
        string language)
    {
        var token = _jwtService.GenerateToken(
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
            ExpiresAt    = expiry
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
