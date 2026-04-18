using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using IGMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IGMS.Infrastructure.Jobs;

/// <summary>
/// Background service that sends a monthly/quarterly compliance coverage report
/// to the configured compliance officer per tenant.
/// Checks daily; sends only on the configured day of month.
/// </summary>
public class ComplianceReportJob : BackgroundService
{
    private readonly ITenantConfigLoader _tenantLoader;
    private readonly ILoggerFactory      _loggerFactory;
    private readonly ILogger<ComplianceReportJob> _log;
    private readonly string              _tenantsDirectory;

    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    public ComplianceReportJob(
        ITenantConfigLoader tenantLoader,
        ILoggerFactory loggerFactory,
        string tenantsDirectory)
    {
        _tenantLoader     = tenantLoader;
        _loggerFactory    = loggerFactory;
        _log              = loggerFactory.CreateLogger<ComplianceReportJob>();
        _tenantsDirectory = tenantsDirectory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("ComplianceReportJob started. First run in {Delay}.", InitialDelay);
        await Task.Delay(InitialDelay, ct);

        while (!ct.IsCancellationRequested)
        {
            try { await RunForAllTenantsAsync(ct); }
            catch (Exception ex)
            { _log.LogError(ex, "ComplianceReportJob: unhandled error"); }

            await Task.Delay(CheckInterval, ct);
        }
    }

    // ── Per-tenant scan ───────────────────────────────────────────────────────

    private async Task RunForAllTenantsAsync(CancellationToken ct)
    {
        if (!Directory.Exists(_tenantsDirectory)) return;

        foreach (var file in Directory.GetFiles(_tenantsDirectory, "*.json"))
        {
            var key = Path.GetFileNameWithoutExtension(file);
            try { await ProcessTenantAsync(key, ct); }
            catch (Exception ex)
            { _log.LogError(ex, "ComplianceReportJob failed for tenant {Key}", key); }
        }
    }

    private async Task ProcessTenantAsync(string tenantKey, CancellationToken ct)
    {
        var tenantCtx = await _tenantLoader.LoadAsync(tenantKey);
        if (tenantCtx is null) return;

        var cfg = tenantCtx.ComplianceReporting;
        if (!cfg.Enabled) return;
        if (string.IsNullOrWhiteSpace(cfg.RecipientEmail)) return;

        // Only send on the configured day of month
        var today = DateTime.UtcNow.Date;
        if (today.Day != Math.Clamp(cfg.DayOfMonth, 1, 28)) return;

        // If Quarterly: only send in Q start months (Jan, Apr, Jul, Oct)
        if (cfg.Schedule.Equals("Quarterly", StringComparison.OrdinalIgnoreCase))
        {
            if (today.Month % 3 != 1) return;
        }

        _log.LogInformation("ComplianceReportJob: building report for tenant {Key}", tenantKey);

        var dbOptions = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(tenantCtx.ConnectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3))
            .Options;

        await using var db = new TenantDbContext(dbOptions, tenantCtx);

        var report = await BuildReportAsync(db, today, tenantCtx.Organization.NameAr, ct);

        var emailLog  = _loggerFactory.CreateLogger<MailKitEmailService>();
        var notifLog  = _loggerFactory.CreateLogger<NotificationService>();
        var emailSvc  = new MailKitEmailService(tenantCtx, emailLog);
        var notifySvc = new NotificationService(emailSvc, notifLog);

        await notifySvc.ComplianceReportAsync(
            cfg.RecipientEmail,
            cfg.RecipientName,
            report,
            tenantCtx.Organization.NameAr);

        _log.LogInformation("ComplianceReportJob: report sent to {Email} for tenant {Key}",
            cfg.RecipientEmail, tenantKey);
    }

    // ── Data aggregation ──────────────────────────────────────────────────────

    public static async Task<ComplianceReportData> BuildReportAsync(
        TenantDbContext db, DateTime reportDate, string orgName, CancellationToken ct)
    {
        var polTotal = await db.Policies.CountAsync(p => !p.IsDeleted, ct);
        var rskTotal = await db.Risks.CountAsync(r => !r.IsDeleted, ct);

        // Mappings per framework
        var allMappings = await db.ComplianceMappings
            .Where(m => !m.IsDeleted)
            .Select(m => new { m.Framework, m.EntityType, m.EntityId })
            .ToListAsync(ct);

        var frameworks = Enum.GetValues<ComplianceFramework>()
            .Select(fw =>
            {
                var fwMappings = allMappings.Where(m => m.Framework == fw).ToList();
                var polCovered = fwMappings.Where(m => m.EntityType == "Policy")
                                           .Select(m => m.EntityId).Distinct().Count();
                var rskCovered = fwMappings.Where(m => m.EntityType == "Risk")
                                           .Select(m => m.EntityId).Distinct().Count();

                double polPct = polTotal > 0 ? Math.Round((double)polCovered / polTotal * 100, 1) : 0;
                double rskPct = rskTotal > 0 ? Math.Round((double)rskCovered / rskTotal * 100, 1) : 0;
                double overall = Math.Round((polPct + rskPct) / 2, 1);

                return new FrameworkCoverage
                {
                    Framework    = fw,
                    Label        = FrameworkLabel(fw),
                    Group        = FrameworkGroup(fw),
                    PolCovered   = polCovered,
                    PolTotal     = polTotal,
                    PolPct       = polPct,
                    RskCovered   = rskCovered,
                    RskTotal     = rskTotal,
                    RskPct       = rskPct,
                    OverallPct   = overall,
                };
            })
            .OrderByDescending(f => f.OverallPct)
            .ToList();

        double avgCompliance = frameworks.Count > 0
            ? Math.Round(frameworks.Average(f => f.OverallPct), 1)
            : 0;

        return new ComplianceReportData
        {
            ReportDate     = reportDate,
            OrgName        = orgName,
            PolTotal       = polTotal,
            RskTotal       = rskTotal,
            Frameworks     = frameworks,
            AvgCompliance  = avgCompliance,
        };
    }

    // ── Framework metadata ────────────────────────────────────────────────────

    private static string FrameworkLabel(ComplianceFramework fw) => fw switch
    {
        ComplianceFramework.Iso31000  => "ISO 31000",
        ComplianceFramework.Cobit2019 => "COBIT 2019",
        ComplianceFramework.UaeNesa   => "UAE NESA",
        ComplianceFramework.Iso27001  => "ISO 27001",
        ComplianceFramework.NiasUae   => "NIAS UAE",
        ComplianceFramework.Adaa      => "ADAA",
        ComplianceFramework.Tdra      => "TDRA",
        ComplianceFramework.UaeIa     => "UAE IA",
        ComplianceFramework.DubaiSm   => "DSM",
        ComplianceFramework.Custom    => "مخصص",
        _                             => fw.ToString(),
    };

    private static string FrameworkGroup(ComplianceFramework fw) => fw switch
    {
        ComplianceFramework.UaeNesa or
        ComplianceFramework.NiasUae or
        ComplianceFramework.Adaa    or
        ComplianceFramework.Tdra    or
        ComplianceFramework.UaeIa   or
        ComplianceFramework.DubaiSm => "إماراتي",
        _                           => "دولي",
    };
}

