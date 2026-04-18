using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IControlTestService
{
    Task<Result<PagedResult<ControlTestListDto>>> GetPagedAsync(ControlTestQuery query);
    Task<Result<ControlTestDetailDto>>            GetByIdAsync(int id);
    Task<Result<ControlTestDetailDto>>            SaveAsync(SaveControlTestRequest request, string by);
    Task<Result<bool>>                            DeleteAsync(int id, string by);

    // Evidence
    Task<Result<ControlEvidenceDto>> UploadEvidenceAsync(
        int controlTestId, Stream stream, string fileName,
        string contentType, long fileSize, string tenantKey, string uploadedBy);

    Task<Result<DownloadResult>> DownloadEvidenceAsync(int evidenceId);
    Task<Result<bool>>           DeleteEvidenceAsync(int evidenceId);
}
