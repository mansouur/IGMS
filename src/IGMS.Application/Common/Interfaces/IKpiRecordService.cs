using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IKpiRecordService
{
    /// <summary>سجل تاريخ قيم مؤشر أداء مرتبة من الأقدم للأحدث.</summary>
    Task<List<KpiRecordDto>> GetHistoryAsync(int kpiId);

    /// <summary>إضافة أو تحديث قيمة لفترة زمنية محددة (upsert).</summary>
    Task<Result<KpiRecordDto>> UpsertAsync(AddKpiRecordRequest req, string recordedBy);

    /// <summary>حذف سجل.</summary>
    Task<Result<bool>> DeleteAsync(int recordId, string deletedBy);
}
