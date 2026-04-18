using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

public class KpiRecordService : IKpiRecordService
{
    private readonly TenantDbContext _db;
    public KpiRecordService(TenantDbContext db) => _db = db;

    public async Task<List<KpiRecordDto>> GetHistoryAsync(int kpiId) =>
        await _db.KpiRecords
            .AsNoTracking()
            .Where(r => r.KpiId == kpiId)
            .OrderBy(r => r.Year).ThenBy(r => r.Quarter)
            .Select(r => new KpiRecordDto
            {
                Id          = r.Id,
                KpiId       = r.KpiId,
                Year        = r.Year,
                Quarter     = r.Quarter,
                TargetValue = r.TargetValue,
                ActualValue = r.ActualValue,
                Notes       = r.Notes,
                RecordedAt  = r.RecordedAt,
                RecordedBy  = r.RecordedBy,
            })
            .ToListAsync();

    public async Task<Result<KpiRecordDto>> UpsertAsync(AddKpiRecordRequest req, string recordedBy)
    {
        if (!await _db.Kpis.AnyAsync(k => k.Id == req.KpiId && !k.IsDeleted))
            return Result<KpiRecordDto>.Failure("مؤشر الأداء غير موجود.");

        // Upsert: update if same period exists, else insert
        var existing = await _db.KpiRecords
            .FirstOrDefaultAsync(r => r.KpiId   == req.KpiId
                                   && r.Year     == req.Year
                                   && r.Quarter  == req.Quarter);
        if (existing is not null)
        {
            existing.TargetValue = req.TargetValue;
            existing.ActualValue = req.ActualValue;
            existing.Notes       = req.Notes;
            existing.RecordedAt  = DateTime.UtcNow;
            existing.RecordedBy  = recordedBy;
            existing.ModifiedAt  = DateTime.UtcNow;
            existing.ModifiedBy  = recordedBy;
        }
        else
        {
            existing = new KpiRecord
            {
                KpiId       = req.KpiId,
                Year        = req.Year,
                Quarter     = req.Quarter,
                TargetValue = req.TargetValue,
                ActualValue = req.ActualValue,
                Notes       = req.Notes,
                RecordedAt  = DateTime.UtcNow,
                RecordedBy  = recordedBy,
                CreatedAt   = DateTime.UtcNow,
                CreatedBy   = recordedBy,
            };
            _db.KpiRecords.Add(existing);
        }

        await _db.SaveChangesAsync();

        return Result<KpiRecordDto>.Success(new KpiRecordDto
        {
            Id          = existing.Id,
            KpiId       = existing.KpiId,
            Year        = existing.Year,
            Quarter     = existing.Quarter,
            TargetValue = existing.TargetValue,
            ActualValue = existing.ActualValue,
            Notes       = existing.Notes,
            RecordedAt  = existing.RecordedAt,
            RecordedBy  = existing.RecordedBy,
        });
    }

    public async Task<Result<bool>> DeleteAsync(int recordId, string deletedBy)
    {
        var record = await _db.KpiRecords.FindAsync(recordId);
        if (record is null) return Result<bool>.Failure("السجل غير موجود.");

        record.IsDeleted  = true;
        record.DeletedAt  = DateTime.UtcNow;
        record.DeletedBy  = deletedBy;
        record.ModifiedAt = DateTime.UtcNow;
        record.ModifiedBy = deletedBy;

        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
}
