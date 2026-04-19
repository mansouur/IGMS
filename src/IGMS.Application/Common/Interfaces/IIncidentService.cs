using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IIncidentService
{
    Task<PagedResult<IncidentListDto>> GetPagedAsync(IncidentQuery query);
    Task<IncidentDetailDto?>     GetByIdAsync(int id);
    Task<IncidentDetailDto>      CreateAsync(SaveIncidentRequest req, int reportedById);
    Task<IncidentDetailDto>      UpdateAsync(int id, SaveIncidentRequest req);
    Task                         DeleteAsync(int id);
    Task<IncidentDetailDto>      ResolveAsync(int id, string? resolutionNotes);
    Task<byte[]>                 ExportAsync(string? status, string? severity);
}
