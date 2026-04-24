using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IPdplService
{
    // Records
    Task<PagedResult<PdplRecordListDto>> GetPagedAsync(PdplQuery query);
    Task<PdplRecordDetailDto?>           GetByIdAsync(int id);
    Task<PdplRecordDetailDto>            CreateAsync(SavePdplRecordRequest req, int createdById);
    Task<PdplRecordDetailDto>            UpdateAsync(int id, SavePdplRecordRequest req);
    Task                                 DeleteAsync(int id);
    Task<PdplRecordDetailDto>            MarkReviewedAsync(int id);

    // Consents
    Task<PdplConsentDto>  AddConsentAsync(int recordId, SaveConsentRequest req);
    Task<PdplConsentDto>  WithdrawConsentAsync(int recordId, int consentId);

    // Data Requests
    Task<PagedResult<PdplDataRequestDto>> GetRequestsAsync(PdplRequestQuery query);
    Task<PdplDataRequestDto>              AddRequestAsync(int recordId, SaveDataRequestRequest req, int createdById);
    Task<PdplDataRequestDto>              ResolveRequestAsync(int recordId, int requestId, ResolveDataRequestRequest req);
}
