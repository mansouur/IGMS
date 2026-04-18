namespace IGMS.Application.Common.Interfaces;

/// <summary>
/// Low-level email sender scoped to the current tenant's SMTP configuration.
/// Delivery failures must never surface as HTTP errors – always wrap in try/catch.
/// If SMTP is not configured for the tenant the call is silently skipped.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
}
