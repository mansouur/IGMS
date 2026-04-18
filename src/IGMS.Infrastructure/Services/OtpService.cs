using IGMS.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace IGMS.Infrastructure.Services;

/// <summary>
/// Email OTP service backed by IDistributedCache (in-memory dev / Redis prod).
/// OTPs are 6-digit numeric codes valid for 5 minutes.
/// Each OTP is consumed on first successful validation (prevent replay).
/// </summary>
public class OtpService : IOtpService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public OtpService(IDistributedCache cache) => _cache = cache;

    public async Task<string> GenerateAndStoreAsync(int userId)
    {
        var otp = Random.Shared.Next(100_000, 1_000_000).ToString("D6");
        await _cache.SetStringAsync(
            CacheKey(userId), otp,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl });
        return otp;
    }

    public async Task<bool> ValidateAndConsumeAsync(int userId, string otp)
    {
        var stored = await _cache.GetStringAsync(CacheKey(userId));
        if (stored is null || stored != otp.Trim()) return false;

        await _cache.RemoveAsync(CacheKey(userId));
        return true;
    }

    private static string CacheKey(int userId) => $"otp:{userId}";
}
