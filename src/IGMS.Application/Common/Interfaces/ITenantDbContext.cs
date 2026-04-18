namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Abstraction over EF DbContext for the tenant database.
/// Kept minimal – Application layer has no dependency on EF Core (Clean Architecture).
/// Infrastructure services inject TenantDbContext directly.
/// </summary>
public interface ITenantDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
