using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Session management backed by IDistributedCache.
/// Development: in-memory. Production: swap to Redis with one config line.
/// </summary>
public interface ISessionService
{
    Task<string> CreateSessionAsync(SessionData data);
    Task<SessionData?> GetSessionAsync(string sessionId);
    Task RevokeSessionAsync(string sessionId);
}
