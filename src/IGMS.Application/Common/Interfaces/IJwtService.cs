using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Handles JWT token generation.
/// Token carries: TenantKey, UserId, Roles, Language.
/// </summary>
public interface IJwtService
{
    string GenerateToken(string userId, string username, List<string> roles, List<string> permissions, string tenantKey, string language);
    DateTime GetTokenExpiry();
}
