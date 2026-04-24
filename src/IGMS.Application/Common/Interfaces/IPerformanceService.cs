using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IPerformanceService
{
    Task<PagedResult<PerformanceReviewListDto>> GetPagedAsync(PerformanceQuery query);
    Task<PerformanceReviewDetailDto?>           GetByIdAsync(int id);
    Task<PerformanceReviewDetailDto>            CreateAsync(SavePerformanceReviewRequest req, int createdById);
    Task<PerformanceReviewDetailDto>            UpdateAsync(int id, SavePerformanceReviewRequest req);
    Task<PerformanceReviewDetailDto>            SubmitAsync(int id);
    Task<PerformanceReviewDetailDto>            ApproveAsync(int id);
    Task<PerformanceReviewDetailDto>            RejectAsync(int id, string? reason);
    Task                                        DeleteAsync(int id);
}
