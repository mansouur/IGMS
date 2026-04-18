using System.Text.Json;
using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace IGMS.Infrastructure.Services;

/// <summary>
/// Session management using IDistributedCache.
///
/// To switch from Memory → Redis: change one line in DependencyInjection.cs:
///   services.AddDistributedMemoryCache()
///   → services.AddStackExchangeRedisCache(o => o.Configuration = "redis:6379")
///
/// Session key format: "igms:session:{sessionId}"
/// </summary>
public class SessionService : ISessionService
{
    private const string KeyPrefix = "igms:session:";

    private readonly IDistributedCache _cache;

    public SessionService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string> CreateSessionAsync(SessionData data)
    {
        data.SessionId = Guid.NewGuid().ToString("N"); // 32-char hex, no dashes

        var json = JsonSerializer.Serialize(data);
        var ttl = data.ExpiresAt - DateTime.UtcNow;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl > TimeSpan.Zero ? ttl : TimeSpan.FromHours(8)
        };

        await _cache.SetStringAsync(BuildKey(data.SessionId), json, options);

        return data.SessionId;
    }

    public async Task<SessionData?> GetSessionAsync(string sessionId)
    {
        var json = await _cache.GetStringAsync(BuildKey(sessionId));
        return json is null ? null : JsonSerializer.Deserialize<SessionData>(json);
    }

    public async Task RevokeSessionAsync(string sessionId)
    {
        await _cache.RemoveAsync(BuildKey(sessionId));
    }

    private static string BuildKey(string sessionId) => $"{KeyPrefix}{sessionId}";
}
