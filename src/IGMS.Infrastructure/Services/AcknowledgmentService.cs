using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class AcknowledgmentService : IAcknowledgmentService
{
    private readonly TenantDbContext _db;
    public AcknowledgmentService(TenantDbContext db) => _db = db;

    public async Task<Result<AcknowledgmentStatusDto>> AcknowledgeAsync(
        int policyId, int userId, string? ipAddress)
    {
        var policyExists = await _db.Policies
            .AnyAsync(p => p.Id == policyId && !p.IsDeleted);

        if (!policyExists)
            return Result<AcknowledgmentStatusDto>.Failure("السياسة غير موجودة.");

        // Upsert: update if exists, insert if new
        var existing = await _db.PolicyAcknowledgments
            .FirstOrDefaultAsync(a => a.PolicyId == policyId && a.UserId == userId);

        if (existing is not null)
        {
            existing.AcknowledgedAt = DateTime.UtcNow;
            existing.IpAddress      = ipAddress;
        }
        else
        {
            _db.PolicyAcknowledgments.Add(new PolicyAcknowledgment
            {
                PolicyId       = policyId,
                UserId         = userId,
                AcknowledgedAt = DateTime.UtcNow,
                IpAddress      = ipAddress,
            });
        }

        await _db.SaveChangesAsync();

        var status = await GetStatusAsync(policyId, userId);
        return Result<AcknowledgmentStatusDto>.Success(status);
    }

    public async Task<AcknowledgmentStatusDto> GetStatusAsync(int policyId, int userId)
    {
        var record = await _db.PolicyAcknowledgments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.PolicyId == policyId && a.UserId == userId);

        var total = await _db.PolicyAcknowledgments
            .AsNoTracking()
            .CountAsync(a => a.PolicyId == policyId);

        return new AcknowledgmentStatusDto
        {
            HasAcknowledged   = record is not null,
            AcknowledgedAt    = record?.AcknowledgedAt,
            TotalAcknowledged = total,
        };
    }

    public async Task<List<AcknowledgmentRecordDto>> GetRecordsAsync(int policyId) =>
        await _db.PolicyAcknowledgments
            .AsNoTracking()
            .Where(a => a.PolicyId == policyId)
            .Include(a => a.User)
            .OrderByDescending(a => a.AcknowledgedAt)
            .Select(a => new AcknowledgmentRecordDto
            {
                Id             = a.Id,
                UserId         = a.UserId,
                FullNameAr     = a.User.FullNameAr,
                Username       = a.User.Username,
                AcknowledgedAt = a.AcknowledgedAt,
            })
            .ToListAsync();
}
