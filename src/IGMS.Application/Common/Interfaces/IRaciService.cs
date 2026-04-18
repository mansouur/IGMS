using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;

namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// RACI module service contract.
/// All operations return Result<T> – no exceptions propagate to the controller.
/// </summary>
public interface IRaciService
{
    Task<Result<PagedResult<RaciMatrixListDto>>> GetPagedAsync(RaciMatrixQuery query);
    Task<Result<RaciMatrixDetailDto>>           GetByIdAsync(int id);
    Task<Result<RaciMatrixDetailDto>>           CreateAsync(CreateRaciMatrixRequest request, string createdBy);
    Task<Result<RaciMatrixDetailDto>>           UpdateAsync(UpdateRaciMatrixRequest request, string modifiedBy);
    Task<Result<bool>>                          DeleteAsync(int id, string deletedBy);
    Task<Result<RaciMatrixDetailDto>>           ApproveAsync(int id, string approvedByUsername, int approvedByUserId);
    Task<Result<bool>>                          SubmitForReviewAsync(int id, string modifiedBy);
}
