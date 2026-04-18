using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Handles user authentication.
/// Supports AD (Active Directory) and Local fallback based on tenant config.
/// </summary>
public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, TenantContext tenant);
}
