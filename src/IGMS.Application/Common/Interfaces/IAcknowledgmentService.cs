using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

public interface IAcknowledgmentService
{
    /// <summary>Acknowledge (or re-acknowledge) a policy for the current user.</summary>
    Task<Result<AcknowledgmentStatusDto>> AcknowledgeAsync(int policyId, int userId, string? ipAddress);

    /// <summary>Get acknowledgment status for the current user on a policy.</summary>
    Task<AcknowledgmentStatusDto> GetStatusAsync(int policyId, int userId);

    /// <summary>List all users who have acknowledged a policy (for managers).</summary>
    Task<List<AcknowledgmentRecordDto>> GetRecordsAsync(int policyId);
}
