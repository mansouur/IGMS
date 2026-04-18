using System.ComponentModel.DataAnnotations;

namespace IGMS.Application.Common.Models;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>Preferred language for the session: "ar" or "en"</summary>
    public string Language { get; set; } = "ar";
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;  // Redis/Memory session key
    public string AuthProvider { get; set; } = "Local";    // Local | AD | UaePass
    public string Username { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public DateTime ExpiresAt { get; set; }
    public string Language { get; set; } = "ar";
    public TenantOrganization Organization { get; set; } = new();

    // ── Two-Factor Authentication ─────────────────────────────────────────────
    /// <summary>True when 2FA is required – client must call /auth/verify-otp.</summary>
    public bool RequiresOtp { get; set; } = false;
    /// <summary>Temporary userId used to call /auth/verify-otp when RequiresOtp = true.</summary>
    public int? PendingUserId { get; set; }
}

public class VerifyOtpRequest
{
    [Required] public int    UserId { get; set; }
    [Required] public string Otp    { get; set; } = string.Empty;
    public string Language { get; set; } = "ar";
}

public class Toggle2FaRequest
{
    public bool Enabled { get; set; }
    /// <summary>Current password – required to toggle 2FA for security.</summary>
    [Required] public string Password { get; set; } = string.Empty;
}

/// <summary>Returned by GET /api/v1/auth/uaepass/redirect</summary>
public class UaePassRedirectResponse
{
    public string RedirectUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
