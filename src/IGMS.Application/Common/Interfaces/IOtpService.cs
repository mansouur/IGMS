namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Generates and validates time-limited one-time passwords (OTPs).
/// Used for Two-Factor Authentication via email.
/// OTPs expire after 5 minutes and are consumed on successful validation.
/// </summary>
public interface IOtpService
{
    /// <summary>Generates a 6-digit OTP, stores it in cache, and returns it.</summary>
    Task<string> GenerateAndStoreAsync(int userId);

    /// <summary>
    /// Validates the OTP. If valid: removes it from cache (consume-once) and returns true.
    /// Returns false if the OTP is wrong, expired, or already used.
    /// </summary>
    Task<bool> ValidateAndConsumeAsync(int userId, string otp);
}
