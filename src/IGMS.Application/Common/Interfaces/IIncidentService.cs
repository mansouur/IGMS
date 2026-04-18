using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Interfaces;

public interface IIncidentService
{
    Task<List<IncidentListDto>>  GetListAsync(string? status, string? severity, int? departmentId, int? riskId = null);
    Task<IncidentDetailDto?>     GetByIdAsync(int id);
    Task<IncidentDetailDto>      CreateAsync(SaveIncidentRequest req, int reportedById);
    Task<IncidentDetailDto>      UpdateAsync(int id, SaveIncidentRequest req);
    Task                         DeleteAsync(int id);
    Task<IncidentDetailDto>      ResolveAsync(int id, string? resolutionNotes);
}
