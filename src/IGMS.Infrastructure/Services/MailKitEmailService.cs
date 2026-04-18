using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace IGMS.Infrastructure.Services;

/// <summary>
/// Sends email via MailKit using per-tenant SMTP settings from TenantContext.
/// Silently skips if SMTP is not configured.
/// Compatible with Mailtrap sandbox (development) and any real SMTP (production).
/// </summary>
public class MailKitEmailService : IEmailService
{
    private readonly TenantContext   _tenant;
    private readonly ILogger<MailKitEmailService> _log;

    public MailKitEmailService(TenantContext tenant, ILogger<MailKitEmailService> log)
    {
        _tenant = tenant;
        _log    = log;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var smtp = _tenant.Smtp;

        if (!smtp.IsConfigured)
        {
            _log.LogDebug("Email skipped (SMTP not configured for tenant {Tenant})", _tenant.TenantKey);
            return;
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _log.LogDebug("Email skipped – recipient email is empty (subject: {Subject})", subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtp.FromName, smtp.FromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();

            var secureOption = smtp.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(smtp.Host, smtp.Port, secureOption);

            if (!string.IsNullOrWhiteSpace(smtp.Username))
                await client.AuthenticateAsync(smtp.Username, smtp.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _log.LogInformation("Email sent to {To} | Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to send email to {To} | Subject: {Subject}", toEmail, subject);
        }
    }
}
