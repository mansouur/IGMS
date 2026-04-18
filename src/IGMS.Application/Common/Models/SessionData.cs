namespace IGMS.Application.Common.Models;

/// <summary>
/// Stored in IDistributedCache (Memory now → Redis in production).
/// Key: "session:{sessionId}"
/// TTL: matches JWT expiry.
/// </summary>
public class SessionData
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string TenantKey { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public string Language { get; set; } = "ar";
    public string AuthProvider { get; set; } = "Local"; // Local | AD | UaePass
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}
