using IGMS.Application.Common.Interfaces;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using IGMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure.Jobs;

/// <summary>
/// Background service that runs daily and sends email reminders
/// to policy owners whose policies are expiring in 30 or 7 days.
/// Operates outside HTTP context – creates DbContext directly per tenant.
/// </summary>
public class PolicyExpiryNotificationJob : BackgroundService
{
    private readonly ITenantConfigLoader               _tenantLoader;
    private readonly ILoggerFactory                    _loggerFactory;
    private readonly ILogger<PolicyExpiryNotificationJob> _log;
    private readonly string                            _tenantsDirectory;

    // Run at 07:00 every day; wait this long before the first run
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan Interval     = TimeSpan.FromHours(24);

    public PolicyExpiryNotificationJob(
        ITenantConfigLoader tenantLoader,
        ILoggerFactory loggerFactory,
        string tenantsDirectory)
    {
        _tenantLoader     = tenantLoader;
        _loggerFactory    = loggerFactory;
        _log              = loggerFactory.CreateLogger<PolicyExpiryNotificationJob>();
        _tenantsDirectory = tenantsDirectory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("PolicyExpiryNotificationJob started. First run in {Delay}.", InitialDelay);

        // Small delay so the app finishes startup before we hit the DB
        await Task.Delay(InitialDelay, ct);

        while (!ct.IsCancellationRequested)
        {
            _log.LogInformation("PolicyExpiryNotificationJob: running scan…");
            try
            {
                await RunForAllTenantsAsync(ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "PolicyExpiryNotificationJob: unhandled error in main loop");
            }

            _log.LogInformation("PolicyExpiryNotificationJob: next run in {Interval}.", Interval);
            await Task.Delay(Interval, ct);
        }
    }

    // ── Per-tenant scan ───────────────────────────────────────────────────────

    private async Task RunForAllTenantsAsync(CancellationToken ct)
    {
        if (!Directory.Exists(_tenantsDirectory))
        {
            _log.LogWarning("Tenants directory not found: {Dir}", _tenantsDirectory);
            return;
        }

        var files = Directory.GetFiles(_tenantsDirectory, "*.json");

        foreach (var file in files)
        {
            var tenantKey = Path.GetFileNameWithoutExtension(file);
            try
            {
                await ProcessTenantAsync(tenantKey, ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "PolicyExpiryNotificationJob failed for tenant {Tenant}", tenantKey);
            }
        }
    }

    private async Task ProcessTenantAsync(string tenantKey, CancellationToken ct)
    {
        var tenantCtx = await _tenantLoader.LoadAsync(tenantKey);

        if (tenantCtx is null || string.IsNullOrWhiteSpace(tenantCtx.ConnectionString))
        {
            _log.LogWarning("Skipping tenant {Key}: no connection string.", tenantKey);
            return;
        }

        // Build DbContext directly – no HTTP context available in background jobs
        var dbOptions = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(tenantCtx.ConnectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3))
            .Options;

        await using var db = new TenantDbContext(dbOptions, tenantCtx);

        var today = DateTime.UtcNow.Date;

        // Notify at two checkpoints: 30 days and 7 days before expiry
        // Use a date-window of exactly 1 day so we don't send duplicates
        var checkpoints = new[] { 30, 7 };

        foreach (var days in checkpoints)
        {
            var windowStart = today.AddDays(days);
            var windowEnd   = windowStart.AddDays(1); // [days, days+1)

            var expiring = await db.Policies
                .Include(p => p.Owner)
                .Where(p => !p.IsDeleted
                    && p.Status == PolicyStatus.Active
                    && p.ExpiryDate.HasValue
                    && p.ExpiryDate!.Value.Date >= windowStart
                    && p.ExpiryDate!.Value.Date < windowEnd)
                .ToListAsync(ct);

            if (expiring.Count == 0) continue;

            _log.LogInformation(
                "Tenant {Key}: {Count} policy(ies) expiring in {Days} days – sending notifications.",
                tenantKey, expiring.Count, days);

            // Build services without DI (background job has no HTTP scope)
            var emailLog  = _loggerFactory.CreateLogger<MailKitEmailService>();
            var notifLog  = _loggerFactory.CreateLogger<Services.NotificationService>();
            var emailSvc  = new MailKitEmailService(tenantCtx, emailLog);
            var notifySvc = new Services.NotificationService(emailSvc, notifLog);

            var orgName = tenantCtx.Organization.NameAr;

            foreach (var policy in expiring)
            {
                if (policy.Owner is null) continue;

                await notifySvc.PolicyExpiringAsync(
                    policyTitle:     policy.TitleAr,
                    ownerEmail:      policy.Owner.Email,
                    ownerName:       policy.Owner.FullNameAr,
                    daysRemaining:   days,
                    organizationName: orgName);
            }
        }
    }
}
