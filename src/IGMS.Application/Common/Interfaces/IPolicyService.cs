using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IPolicyService
{
    Task<Result<PagedResult<PolicyListDto>>> GetPagedAsync(PolicyQuery query);
    Task<Result<PolicyDetailDto>>            GetByIdAsync(int id);
    Task<Result<PolicyDetailDto>>            SaveAsync(SavePolicyRequest request, string by);
    Task<Result<bool>>                       DeleteAsync(int id, string by);
    Task<Result<bool>>                       SetStatusAsync(int id, int status, string by, int? approverId = null);
    Task<Result<PolicyDetailDto>>            RenewAsync(int id, string by);
    Task<List<PolicyVersionDto>>             GetVersionsAsync(int id);
    Task<byte[]>                             ExportAsync(PolicyQuery query);
}
