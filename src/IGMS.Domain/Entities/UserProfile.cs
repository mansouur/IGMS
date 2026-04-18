using IGMS.Domain.Common;

namespace IGMS.Domain.Entities;

/// <summary>
/// System user. Supports three auth providers:
///   Local    → PasswordHash (BCrypt)
///   AD       → AdObjectId (from directory)
///   UaePass  → UaePassSub + EmiratesId
/// A user can have multiple providers (e.g. Local + UaePass for same account).
/// </summary>
public class UserProfile : AuditableEntity
{
    public string Username { get; set; } = string.Empty;

    // BCrypt hash – null when user authenticates via AD or UAE Pass only
    public string? PasswordHash { get; set; }

    public string FullNameAr { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImagePath { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Two-Factor Authentication via email OTP.</summary>
    public bool TwoFactorEnabled { get; set; } = false;

    // ── Auth Provider Fields ──────────────────────────────────────────────────

    /// <summary>AD ObjectGuid – populated when auth mode is AD</summary>
    public string? AdObjectId { get; set; }

    /// <summary>UAE Pass unique subject identifier (sub claim)</summary>
    public string? UaePassSub { get; set; }

    /// <summary>UAE Pass Emirates ID (e.g. 784-1990-1234567-8)</summary>
    public string? EmiratesId { get; set; }

    // ── Relationships ─────────────────────────────────────────────────────────

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
