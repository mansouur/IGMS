using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IRiskService
{
    Task<Result<PagedResult<RiskListDto>>> GetPagedAsync(RiskQuery query);
    Task<Result<RiskDetailDto>>            GetByIdAsync(int id);
    Task<Result<RiskDetailDto>>            SaveAsync(SaveRiskRequest request, string by);
    Task<Result<bool>>                     DeleteAsync(int id, string by);
    Task<byte[]>                           ExportAsync(RiskQuery query);
    Task<List<RiskHeatMapItemDto>>         GetHeatMapAsync();
    Task<List<RiskKpiLinkDto>>             GetKpiLinksAsync(int riskId);
    Task<RiskKpiLinkDto>                   AddKpiLinkAsync(int riskId, int kpiId, string? notes);
    Task                                   RemoveKpiLinkAsync(int mappingId);
}
