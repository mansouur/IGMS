using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Auth.Strategies;
using IGMS.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IGMS.Tests.Auth;

public class LocalAuthStrategyTests
{
    // ── Setup helpers ─────────────────────────────────────────────────────────

    private static (LocalAuthStrategy strategy, Mock<IJwtService> jwtMock) BuildStrategy(string dbName)
    {
        var db          = DbContextFactory.Create(dbName);
        var jwtMock     = new Mock<IJwtService>();
        var sessionMock = new Mock<ISessionService>();
        var otpMock     = new Mock<IOtpService>();
        var notifyMock  = new Mock<INotificationService>();
        var logger      = NullLogger<LocalAuthStrategy>.Instance;

        jwtMock.Setup(j => j.GenerateToken(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .Returns("test-jwt-token");

        jwtMock.Setup(j => j.GetTokenExpiry())
            .Returns(DateTime.UtcNow.AddHours(8));

        sessionMock.Setup(s => s.CreateSessionAsync(It.IsAny<SessionData>()))
            .ReturnsAsync(Guid.NewGuid().ToString());

        otpMock.Setup(o => o.GenerateAndStoreAsync(It.IsAny<int>()))
            .ReturnsAsync("123456");

        var strategy = new LocalAuthStrategy(
            db, jwtMock.Object, sessionMock.Object,
            otpMock.Object, notifyMock.Object, logger);

        return (strategy, jwtMock);
    }

    private static TenantContext TestTenant() => new()
    {
        TenantKey    = "test",
        Organization = new TenantOrganization { NameAr = "جهة اختبار" },
    };

    private static UserProfile MakeUser(string dbName, bool twoFactor = false)
    {
        using var db = DbContextFactory.Create(dbName);

        var perm = new Permission { Code = "RISKS.READ", Module = "Risks", Action = "Read" };
        var role = new Role      { Code = "USER", NameAr = "مستخدم" };
        role.RolePermissions.Add(new RolePermission { Permission = perm });

        var user = new UserProfile
        {
            Username         = "testuser",
            FullNameAr       = "مستخدم اختبار",
            Email            = "test@igms.local",
            PasswordHash     = BCrypt.Net.BCrypt.HashPassword("Correct@123"),
            IsActive         = true,
            TwoFactorEnabled = twoFactor,
        };
        user.UserRoles.Add(new UserRole { Role = role }); // IsActive = true when ExpiresAt is null

        db.UserProfiles.Add(user);
        db.SaveChanges();
        return user;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AuthenticateAsync_UserNotFound_ReturnsFailure()
    {
        var (strategy, _) = BuildStrategy(nameof(AuthenticateAsync_UserNotFound_ReturnsFailure));
        var req = new LoginRequest { Username = "nobody", Password = "pass" };

        var result = await strategy.AuthenticateAsync(req, TestTenant());

        Assert.False(result.IsSuccess);
        Assert.Contains("اسم المستخدم", result.Error);
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ReturnsFailure()
    {
        var dbName = nameof(AuthenticateAsync_WrongPassword_ReturnsFailure);
        MakeUser(dbName);
        var (strategy, _) = BuildStrategy(dbName);
        var req = new LoginRequest { Username = "testuser", Password = "WrongPassword!" };

        var result = await strategy.AuthenticateAsync(req, TestTenant());

        Assert.False(result.IsSuccess);
        Assert.Contains("اسم المستخدم", result.Error);
    }

    [Fact]
    public async Task AuthenticateAsync_CorrectCredentials_ReturnsToken()
    {
        var dbName = nameof(AuthenticateAsync_CorrectCredentials_ReturnsToken);
        MakeUser(dbName);
        var (strategy, jwtMock) = BuildStrategy(dbName);
        var req = new LoginRequest { Username = "testuser", Password = "Correct@123", Language = "ar" };

        var result = await strategy.AuthenticateAsync(req, TestTenant());

        Assert.True(result.IsSuccess);
        Assert.Equal("test-jwt-token", result.Value!.Token);
        Assert.False(result.Value.RequiresOtp);
        jwtMock.Verify(j => j.GenerateToken(
            It.IsAny<string>(), "testuser",
            It.IsAny<List<string>>(), It.IsAny<List<string>>(),
            "test", "ar"), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_TwoFactorEnabled_ReturnsRequiresOtp()
    {
        var dbName = nameof(AuthenticateAsync_TwoFactorEnabled_ReturnsRequiresOtp);
        MakeUser(dbName, twoFactor: true);
        var (strategy, jwtMock) = BuildStrategy(dbName);
        var req = new LoginRequest { Username = "testuser", Password = "Correct@123" };

        var result = await strategy.AuthenticateAsync(req, TestTenant());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.RequiresOtp);
        Assert.True(string.IsNullOrEmpty(result.Value.Token)); // لا JWT قبل OTP
        jwtMock.Verify(j => j.GenerateToken(    // لا يُستدعى GenerateToken
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<List<string>>(), It.IsAny<List<string>>(),
            It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void CollectRolesAndPermissions_ReturnsDistinctValues()
    {
        var perm1 = new Permission { Code = "RISKS.READ",   Module = "Risks",    Action = "Read" };
        var perm2 = new Permission { Code = "RISKS.CREATE", Module = "Risks",    Action = "Create" };
        var perm3 = new Permission { Code = "RISKS.READ",   Module = "Risks",    Action = "Read" }; // مكرر

        var role1 = new Role { Code = "USER",  NameAr = "مستخدم" };
        var role2 = new Role { Code = "ADMIN", NameAr = "مدير" };
        role1.RolePermissions.Add(new RolePermission { Permission = perm1 });
        role1.RolePermissions.Add(new RolePermission { Permission = perm3 });
        role2.RolePermissions.Add(new RolePermission { Permission = perm2 });

        var user = new UserProfile { Username = "u", FullNameAr = "u", Email = "u@u.com" };
        user.UserRoles.Add(new UserRole { Role = role1 }); // IsActive = true when ExpiresAt is null
        user.UserRoles.Add(new UserRole { Role = role2 });

        var (roles, perms) = LocalAuthStrategy.CollectRolesAndPermissions(user);

        Assert.Equal(2, roles.Count);
        Assert.Contains("USER",  roles);
        Assert.Contains("ADMIN", roles);

        // RISKS.READ يظهر مرة واحدة رغم تكراره
        Assert.Equal(2, perms.Count);
        Assert.Contains("RISKS.READ",   perms);
        Assert.Contains("RISKS.CREATE", perms);
    }

    [Fact]
    public void CollectRolesAndPermissions_InactiveRoles_Excluded()
    {
        var role = new Role { Code = "ADMIN", NameAr = "مدير" };
        role.RolePermissions.Add(new RolePermission
        {
            Permission = new Permission { Code = "ALL", Module = "All", Action = "All" }
        });

        var user = new UserProfile { Username = "u", FullNameAr = "u", Email = "u@u.com" };
        user.UserRoles.Add(new UserRole { Role = role, ExpiresAt = DateTime.UtcNow.AddDays(-1) }); // غير نشط

        var (roles, perms) = LocalAuthStrategy.CollectRolesAndPermissions(user);

        Assert.Empty(roles);
        Assert.Empty(perms);
    }
}
