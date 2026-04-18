using IGMS.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IGMS.Infrastructure.Persistence;

/// <summary>
/// Used ONLY by EF Core CLI tools (dotnet ef migrations / database update).
/// Never runs in production – provides a hardcoded dev connection for design-time.
/// Connection string must point to the dev tenant DB on MANSOUR.
/// </summary>
public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        const string devConnection =
            "Server=MANSOUR;Database=IGMS_UAE_SPORT;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(devConnection, sql => sql.EnableRetryOnFailure(3))
            .Options;

        var devTenant = new TenantContext
        {
            TenantKey        = "uae-sport",
            ConnectionString = devConnection
        };

        return new TenantDbContext(options, devTenant);
    }
}
