using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;
using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure.Auth.Strategies;

/// <summary>
/// Active Directory authentication via LDAP.
/// Activated when tenant config: authentication.mode = "AD" or "Mixed".
/// Requires tenant config: authentication.domain (e.g. "sport.gov.ae").
/// </summary>
public class AdAuthStrategy : IAuthStrategy
{
    public string ProviderName => "AD";

    private readonly IJwtService _jwtService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<AdAuthStrategy> _logger;

    public AdAuthStrategy(
        IJwtService jwtService,
        ISessionService sessionService,
        ILogger<AdAuthStrategy> logger)
    {
        _jwtService = jwtService;
        _sessionService = sessionService;
        _logger = logger;
    }

    [SupportedOSPlatform("windows")]
    public async Task<Result<LoginResponse>> AuthenticateAsync(LoginRequest request, TenantContext tenant)
    {
        if (string.IsNullOrEmpty(tenant.Authentication.Domain))
            return Result<LoginResponse>.Failure("AD domain is not configured for this tenant.");

        try
        {
            using var context = new PrincipalContext(
                ContextType.Domain,
                tenant.Authentication.Domain);

            var isValid = context.ValidateCredentials(request.Username, request.Password);

            if (!isValid)
            {
                _logger.LogWarning("AD login failed for user: {Username} on domain: {Domain}",
                    request.Username, tenant.Authentication.Domain);
                return Result<LoginResponse>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            // Fetch AD user details
            using var userPrincipal = UserPrincipal.FindByIdentity(context, request.Username);
            var fullName = userPrincipal?.DisplayName ?? request.Username;
            var userId = userPrincipal?.Guid?.ToString() ?? request.Username;

            // AD groups → IGMS roles mapping
            // TODO (Phase 1): map AD groups to IGMS roles from tenant config
            var roles = new List<string> { "User" };

            return await BuildLoginResponseAsync(
                userId: userId,
                username: request.Username,
                fullNameAr: fullName,
                fullNameEn: fullName,
                roles: roles,
                permissions: [],  // TODO (Phase 2): map AD groups to permissions
                tenant: tenant,
                language: request.Language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AD authentication error for user: {Username}", request.Username);
            return Result<LoginResponse>.Failure("فشل الاتصال بـ Active Directory. تواصل مع مدير النظام.");
        }
    }

    private async Task<Result<LoginResponse>> BuildLoginResponseAsync(
        string userId, string username, string fullNameAr, string fullNameEn,
        List<string> roles, List<string> permissions, TenantContext tenant, string language)
    {
        var token = _jwtService.GenerateToken(userId, username, roles, permissions, tenant.TenantKey, language);
        var expiry = _jwtService.GetTokenExpiry();

        var session = new SessionData
        {
            UserId = userId,
            Username = username,
            FullNameAr = fullNameAr,
            FullNameEn = fullNameEn,
            TenantKey = tenant.TenantKey,
            Roles = roles,
            Language = language,
            AuthProvider = ProviderName,
            ExpiresAt = expiry
        };

        var sessionId = await _sessionService.CreateSessionAsync(session);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            SessionId = sessionId,
            AuthProvider = ProviderName,
            Username = username,
            FullNameAr = fullNameAr,
            FullNameEn = fullNameEn,
            Roles = roles,
            ExpiresAt = expiry,
            Language = language,
            Organization = tenant.Organization
        });
    }
}
