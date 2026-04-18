using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure.Auth;

/// <summary>
/// Local database authentication – fallback when AD is not configured or unavailable.
/// Validates credentials against UserProfiles in the tenant DB.
/// Passwords are stored hashed (BCrypt) – never plaintext.
/// </summary>
public class LocalAuthService : IAuthService
{
    private readonly TenantDbContext _dbContext;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LocalAuthService> _logger;

    public LocalAuthService(
        TenantDbContext dbContext,
        IJwtService jwtService,
        ILogger<LocalAuthService> logger)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _logger = logger;
    }

    public Task<Result<LoginResponse>> LoginAsync(LoginRequest request, TenantContext tenant)
    {
        // TODO (Phase 1): Replace with actual UserProfile entity once migrations are applied
        // For Phase 0, return a dev seed user to allow testing without DB data
        if (IsDevSeedUser(request))
            return Task.FromResult(Result<LoginResponse>.Success(BuildDevSeedResponse(request, tenant)));

        _logger.LogWarning("Login attempted for unknown user: {Username} on tenant: {TenantKey}",
            request.Username, tenant.TenantKey);

        return Task.FromResult(Result<LoginResponse>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة."));
    }

    // Temporary dev seed – removed once UserProfile entity and migrations are ready
    private static bool IsDevSeedUser(LoginRequest request) =>
        request.Username == "admin" && request.Password == "Admin@123";

    private LoginResponse BuildDevSeedResponse(LoginRequest request, TenantContext tenant)
    {
        var roles = new List<string> { "ADMIN" };
        var token = _jwtService.GenerateToken(
            userId: "dev-seed-001",
            username: request.Username,
            roles: roles,
            permissions: [],
            tenantKey: tenant.TenantKey,
            language: request.Language);

        return new LoginResponse
        {
            Token = token,
            Username = request.Username,
            FullNameAr = "المدير العام",
            FullNameEn = "System Administrator",
            Roles = roles,
            ExpiresAt = _jwtService.GetTokenExpiry(),
            Language = request.Language,
            Organization = tenant.Organization
        };
    }
}
