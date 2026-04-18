using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class ComplianceMappingService : IComplianceMappingService
{
    private readonly TenantDbContext _db;
    public ComplianceMappingService(TenantDbContext db) => _db = db;

    public async Task<List<ComplianceMappingDto>> GetByEntityAsync(string entityType, int entityId) =>
        await _db.ComplianceMappings
            .AsNoTracking()
            .Where(c => c.EntityType == entityType && c.EntityId == entityId)
            .OrderBy(c => c.Framework).ThenBy(c => c.Clause)
            .Select(c => new ComplianceMappingDto
            {
                Id             = c.Id,
                EntityType     = c.EntityType,
                EntityId       = c.EntityId,
                Framework      = c.Framework,
                FrameworkLabel = FrameworkLabel(c.Framework),
                Clause         = c.Clause,
                Notes          = c.Notes,
                CreatedAt      = c.CreatedAt,
                CreatedBy      = c.CreatedBy ?? string.Empty,
            })
            .ToListAsync();

    public async Task<Result<ComplianceMappingDto>> AddAsync(AddComplianceMappingRequest req, string by)
    {
        // Prevent duplicate (same entity + framework + clause)
        var exists = await _db.ComplianceMappings.AnyAsync(c =>
            c.EntityType == req.EntityType &&
            c.EntityId   == req.EntityId   &&
            c.Framework  == req.Framework   &&
            c.Clause     == req.Clause);

        if (exists)
            return Result<ComplianceMappingDto>.Failure("هذا الربط موجود مسبقاً.");

        var mapping = new ComplianceMapping
        {
            EntityType = req.EntityType,
            EntityId   = req.EntityId,
            Framework  = req.Framework,
            Clause     = req.Clause,
            Notes      = req.Notes,
            CreatedAt  = DateTime.UtcNow,
            CreatedBy  = by,
        };

        _db.ComplianceMappings.Add(mapping);
        await _db.SaveChangesAsync();

        return Result<ComplianceMappingDto>.Success(new ComplianceMappingDto
        {
            Id             = mapping.Id,
            EntityType     = mapping.EntityType,
            EntityId       = mapping.EntityId,
            Framework      = mapping.Framework,
            FrameworkLabel = FrameworkLabel(mapping.Framework),
            Clause         = mapping.Clause,
            Notes          = mapping.Notes,
            CreatedAt      = mapping.CreatedAt,
            CreatedBy      = mapping.CreatedBy ?? string.Empty,
        });
    }

    public async Task<Result<bool>> DeleteAsync(int id, string by)
    {
        var c = await _db.ComplianceMappings.FindAsync(id);
        if (c is null) return Result<bool>.Failure("الربط غير موجود.");

        c.IsDeleted  = true;
        c.DeletedAt  = DateTime.UtcNow;
        c.DeletedBy  = by;
        c.ModifiedAt = DateTime.UtcNow;
        c.ModifiedBy = by;

        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static string FrameworkLabel(ComplianceFramework f) => f switch
    {
        ComplianceFramework.Iso31000  => "ISO 31000",
        ComplianceFramework.Cobit2019 => "COBIT 2019",
        ComplianceFramework.UaeNesa   => "UAE NESA",
        ComplianceFramework.Iso27001  => "ISO 27001",
        ComplianceFramework.NiasUae   => "NIAS UAE",
        ComplianceFramework.Custom    => "مخصص",
        _                             => f.ToString(),
    };
}
