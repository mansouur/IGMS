using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Interfaces;

public interface IRegulatoryService
{
    Task<List<RegulatoryFrameworkDto>>  GetFrameworksAsync();
    Task<List<RegulatoryControlDto>>    GetControlsByFrameworkAsync(int frameworkId, string? domain = null);
    Task<List<ControlMappingDto>>       GetMappingsForEntityAsync(string entityType, int entityId);
    Task<ControlMappingDto>             CreateMappingAsync(SaveControlMappingRequest req);
    Task<ControlMappingDto>             UpdateMappingAsync(int id, UpdateMappingStatusRequest req);
    Task                                DeleteMappingAsync(int id);
    Task<ComplianceCoverageDto>         GetCoverageAsync(int frameworkId);
}
