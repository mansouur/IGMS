using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IKpiService
{
    Task<Result<PagedResult<KpiListDto>>> GetPagedAsync(KpiQuery query);
    Task<Result<KpiDetailDto>>            GetByIdAsync(int id);
    Task<Result<KpiDetailDto>>            SaveAsync(SaveKpiRequest request, string by);
    Task<Result<bool>>                    DeleteAsync(int id, string by);
    Task<byte[]>                          ExportAsync(KpiQuery query);
    Task<List<KpiRiskLinkDto>>            GetRiskLinksAsync(int kpiId);
}
