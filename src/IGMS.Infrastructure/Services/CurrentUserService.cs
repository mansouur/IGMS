using System.Security.Claims;
using IGMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IGMS.Infrastructure.Services;

/// <summary>
/// Reads the current user's identity from JWT claims injected by ASP.NET Core.
/// Scoped per request – always reflects the authenticated user of the current HTTP call.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    public string UserId =>
        _user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public string Username =>
        _user?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public string TenantKey =>
        _user?.FindFirstValue("tenant_key") ?? string.Empty;

    public string Language =>
        _user?.FindFirstValue("language") ?? "ar";

    public List<string> Roles =>
        _user?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public bool IsAuthenticated =>
        _user?.Identity?.IsAuthenticated ?? false;
}
