using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IComplianceMappingService
{
    Task<List<ComplianceMappingDto>> GetByEntityAsync(string entityType, int entityId);
    Task<Result<ComplianceMappingDto>> AddAsync(AddComplianceMappingRequest req, string by);
    Task<Result<bool>> DeleteAsync(int id, string by);
}
