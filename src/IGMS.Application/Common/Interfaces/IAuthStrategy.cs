using IGMS.Application.Common.Models;
using IGMS.Domain.Common;

namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Strategy interface for authentication providers.
/// Add new providers (SAML, Google Workspace, etc.) without touching existing code.
/// </summary>
public interface IAuthStrategy
{
    /// <summary>Unique provider name: "Local" | "AD" | "UaePass"</summary>
    string ProviderName { get; }

    Task<Result<LoginResponse>> AuthenticateAsync(LoginRequest request, TenantContext tenant);
}
