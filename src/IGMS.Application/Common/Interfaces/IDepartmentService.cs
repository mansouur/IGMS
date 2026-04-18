using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Department (Organizational Structure) module service contract.
/// </summary>
public interface IDepartmentService
{
    Task<Result<PagedResult<DepartmentListDto>>> GetPagedAsync(DepartmentQuery query);
    Task<Result<List<DepartmentTreeDto>>>        GetTreeAsync();
    Task<Result<DepartmentDetailDto>>            GetByIdAsync(int id);
    Task<Result<DepartmentDetailDto>>            CreateAsync(CreateDepartmentRequest request, string createdBy);
    Task<Result<DepartmentDetailDto>>            UpdateAsync(UpdateDepartmentRequest request, string modifiedBy);
    Task<Result<bool>>                           DeleteAsync(int id, string deletedBy);
    Task<Result<bool>>                           SetActiveAsync(int id, bool isActive, string modifiedBy);
}
