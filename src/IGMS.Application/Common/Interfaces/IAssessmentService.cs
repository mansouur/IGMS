using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Interfaces;

public interface IAssessmentService
{
    Task<List<AssessmentListDto>>  GetListAsync(int currentUserId);
    Task<AssessmentDetailDto?>     GetByIdAsync(int id);
    Task<AssessmentDetailDto>      SaveAsync(int? id, SaveAssessmentRequest req, int currentUserId);
    Task                           DeleteAsync(int id);
    Task                           PublishAsync(int id);
    Task                           CloseAsync(int id);

    Task<AssessmentResponseDto?>   GetMyResponseAsync(int assessmentId, int userId);
    Task<AssessmentResponseDto>    SaveResponseAsync(int assessmentId, int userId, int? departmentId, SubmitResponseRequest req, bool submit);

    Task<AssessmentReportDto>      GetReportAsync(int assessmentId);
}
