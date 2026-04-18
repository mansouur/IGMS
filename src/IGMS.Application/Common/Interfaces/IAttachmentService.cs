using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IAttachmentService
{
    Task<Result<PolicyAttachmentDto>> UploadAsync(
        int policyId, Stream stream, string fileName,
        string contentType, long fileSize,
        string tenantKey, string uploadedBy);

    Task<List<PolicyAttachmentDto>> GetByPolicyAsync(int policyId);
    Task<Result<DownloadResult>>    DownloadAsync(int attachmentId);
    Task<Result<bool>>              DeleteAsync(int attachmentId);
}
