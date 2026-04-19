using IGMS.Application.Common.Models;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Tests.Helpers;

/// <summary>
/// Creates an isolated InMemory TenantDbContext for each test.
/// Each call with a unique dbName gets a fresh database.
/// </summary>
public static class DbContextFactory
{
    public static TenantDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        var tenant = new TenantContext
        {
            TenantKey    = "test-tenant",
            Organization = new TenantOrganization { NameAr = "جهة اختبار", NameEn = "Test Org" },
        };

        return new TenantDbContext(options, tenant);
    }
}
