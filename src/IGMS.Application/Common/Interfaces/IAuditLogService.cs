using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Read-side service for the audit trail.
/// Write side is handled automatically in TenantDbContext.SaveChangesAsync.
/// </summary>
public interface IAuditLogService
{
    Task<PagedResult<AuditLogListDto>> GetPagedAsync(AuditLogQuery q);

    /// <summary>Returns the distinct entity types that have audit records.</summary>
    Task<List<string>> GetEntityTypesAsync();
}
