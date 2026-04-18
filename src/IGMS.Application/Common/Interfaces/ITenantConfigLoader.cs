using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Loads tenant configuration from JSON files in the /tenants directory.
/// Each tenant has its own JSON file (e.g., uae-sport.json).
/// </summary>
public interface ITenantConfigLoader
{
    Task<TenantContext?> LoadAsync(string tenantKey);
}
