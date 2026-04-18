namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Provides the identity of the authenticated user for the current request.
/// Extracted from JWT claims – used by audit interceptors and business logic.
/// </summary>
public interface ICurrentUserService
{
    string UserId { get; }
    string Username { get; }
    string TenantKey { get; }
    string Language { get; }
    List<string> Roles { get; }
    bool IsAuthenticated { get; }
}
