using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly TenantDbContext _db;
    public AuditLogService(TenantDbContext db) => _db = db;

    public async Task<PagedResult<AuditLogListDto>> GetPagedAsync(AuditLogQuery q)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.EntityName))
            query = query.Where(a => a.EntityName == q.EntityName);

        if (!string.IsNullOrWhiteSpace(q.EntityId))
            query = query.Where(a => a.EntityId == q.EntityId);

        if (!string.IsNullOrWhiteSpace(q.Action))
            query = query.Where(a => a.Action == q.Action);

        if (q.UserId.HasValue)
            query = query.Where(a => a.UserId == q.UserId.Value);

        if (q.From.HasValue)
            query = query.Where(a => a.Timestamp >= q.From.Value);

        if (q.To.HasValue)
            query = query.Where(a => a.Timestamp <= q.To.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(a => new AuditLogListDto
            {
                Id         = a.Id,
                EntityName = a.EntityName,
                EntityId   = a.EntityId,
                Action     = a.Action,
                OldValues  = a.OldValues,
                NewValues  = a.NewValues,
                UserId     = a.UserId,
                Username   = a.Username,
                IpAddress  = a.IpAddress,
                Timestamp  = a.Timestamp,
            })
            .ToListAsync();

        return PagedResult<AuditLogListDto>.Create(items, total, q.Page, q.PageSize);
    }

    public async Task<List<string>> GetEntityTypesAsync() =>
        await _db.AuditLogs
            .AsNoTracking()
            .Select(a => a.EntityName)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();
}
