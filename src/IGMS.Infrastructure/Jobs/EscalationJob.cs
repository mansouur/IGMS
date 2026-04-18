using IGMS.Application.Common.Interfaces;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using IGMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskStatus = IGMS.Domain.Entities.TaskStatus;

namespace IGMS.Infrastructure.Jobs;

/// <summary>
/// Runs daily and escalates overdue tasks:
/// – Level 1 (day ≥ 1 overdue): one-time reminder email to assignee
/// – Level 2 (day ≥ 4 overdue): one-time escalation email to department manager
///
/// Uses AuditLog to track whether an escalation was already sent,
/// so each task receives at most one L1 and one L2 email over its lifetime.
/// </summary>
public class EscalationJob : BackgroundService
{
    private readonly ITenantConfigLoader      _tenantLoader;
    private readonly ILoggerFactory           _loggerFactory;
    private readonly ILogger<EscalationJob>   _log;
    private readonly string                   _tenantsDirectory;

    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan Interval     = TimeSpan.FromHours(24);

    private const string ActionL1 = "EscalationL1"; // Reminder → assignee
    private const string ActionL2 = "EscalationL2"; // Escalation → manager

    public EscalationJob(
        ITenantConfigLoader tenantLoader,
        ILoggerFactory loggerFactory,
        string tenantsDirectory)
    {
        _tenantLoader     = tenantLoader;
        _loggerFactory    = loggerFactory;
        _log              = loggerFactory.CreateLogger<EscalationJob>();
        _tenantsDirectory = tenantsDirectory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("EscalationJob started. First run in {Delay}.", InitialDelay);
        await Task.Delay(InitialDelay, ct);

        while (!ct.IsCancellationRequested)
        {
            _log.LogInformation("EscalationJob: running scan…");
            try
            {
                await RunForAllTenantsAsync(ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "EscalationJob: unhandled error in main loop");
            }

            _log.LogInformation("EscalationJob: next run in {Interval}.", Interval);
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

        foreach (var file in Directory.GetFiles(_tenantsDirectory, "*.json"))
        {
            var tenantKey = Path.GetFileNameWithoutExtension(file);
            try { await ProcessTenantAsync(tenantKey, ct); }
            catch (Exception ex)
            {
                _log.LogError(ex, "EscalationJob failed for tenant {Tenant}", tenantKey);
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

        // Read per-tenant escalation config (defaults apply if section absent)
        var esc = tenantCtx.Escalation;
        if (!esc.Enabled)
        {
            _log.LogInformation("EscalationJob: escalation disabled for tenant {Key}.", tenantKey);
            return;
        }

        var reminderDays = Math.Max(0, esc.ReminderAfterDays);
        var escalateDays = Math.Max(reminderDays + 1, esc.EscalateAfterDays);

        var dbOptions = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(tenantCtx.ConnectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3))
            .Options;

        await using var db = new TenantDbContext(dbOptions, tenantCtx);

        var today = DateTime.UtcNow.Date;

        // ── Load all overdue active tasks ─────────────────────────────────────
        var overdueTasks = await db.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.Department)
                .ThenInclude(d => d!.Manager)
            .Where(t => !t.IsDeleted
                && t.DueDate.HasValue
                && t.DueDate!.Value.Date < today
                && t.Status != TaskStatus.Done
                && t.Status != TaskStatus.Cancelled
                && t.AssignedToId.HasValue)
            .ToListAsync(ct);

        if (overdueTasks.Count == 0)
        {
            _log.LogInformation("EscalationJob tenant {Key}: no overdue tasks.", tenantKey);
            return;
        }

        _log.LogInformation("EscalationJob tenant {Key}: {Count} overdue task(s).", tenantKey, overdueTasks.Count);

        // ── Load existing escalation audit entries for these tasks ────────────
        var taskIdStrings = overdueTasks.Select(t => t.Id.ToString()).ToHashSet();

        var existingLogs = await db.AuditLogs
            .Where(l => l.EntityName == "GovernanceTask"
                && taskIdStrings.Contains(l.EntityId)
                && (l.Action == ActionL1 || l.Action == ActionL2))
            .Select(l => new { l.EntityId, l.Action })
            .ToListAsync(ct);

        // Build lookup: taskId → set of actions already logged
        var loggedActions = existingLogs
            .GroupBy(l => l.EntityId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(l => l.Action).ToHashSet());

        // ── Build notification services ───────────────────────────────────────
        var emailSvc  = new MailKitEmailService(tenantCtx, _loggerFactory.CreateLogger<MailKitEmailService>());
        var notifySvc = new NotificationService(emailSvc, _loggerFactory.CreateLogger<NotificationService>());
        var orgName   = tenantCtx.Organization.NameAr;

        var newLogs   = new List<AuditLog>();

        foreach (var task in overdueTasks)
        {
            if (task.AssignedTo is null) continue;

            var daysOverdue = (int)(today - task.DueDate!.Value.Date).TotalDays;
            var idStr       = task.Id.ToString();
            loggedActions.TryGetValue(idStr, out var sent);
            sent ??= new HashSet<string>();

            // ── Level 2: Escalate to department manager ───────────────────────
            if (daysOverdue >= escalateDays && !sent.Contains(ActionL2))
            {
                var dept = task.Department;
                var mgr  = dept?.Manager;

                if (mgr is not null && mgr.Id != task.AssignedToId)
                {
                    await notifySvc.TaskEscalatedAsync(
                        taskTitle:        task.TitleAr,
                        assigneeNameAr:   task.AssignedTo.FullNameAr,
                        managerEmail:     mgr.Email,
                        managerName:      mgr.FullNameAr,
                        daysOverdue:      daysOverdue,
                        organizationName: orgName);

                    newLogs.Add(EscalationLog(task.Id, ActionL2));
                    _log.LogInformation(
                        "EscalationJob: L2 sent for task {Id} ({Days}d overdue) → manager {Mgr}.",
                        task.Id, daysOverdue, mgr.FullNameAr);
                }
                continue; // Don't also send L1 if we're already at L2
            }

            // ── Level 1: Reminder to assignee ────────────────────────────────
            if (daysOverdue >= reminderDays && !sent.Contains(ActionL1))
            {
                await notifySvc.TaskOverdueAsync(
                    taskTitle:        task.TitleAr,
                    assigneeEmail:    task.AssignedTo.Email,
                    assigneeName:     task.AssignedTo.FullNameAr,
                    daysOverdue:      daysOverdue,
                    organizationName: orgName);

                newLogs.Add(EscalationLog(task.Id, ActionL1));
                _log.LogInformation(
                    "EscalationJob: L1 reminder sent for task {Id} ({Days}d overdue) → {Assignee}.",
                    task.Id, daysOverdue, task.AssignedTo.FullNameAr);
            }
        }

        // ── Persist all new audit log entries in one batch ────────────────────
        if (newLogs.Count > 0)
        {
            db.AuditLogs.AddRange(newLogs);
            await db.SaveChangesAsync(ct);
            _log.LogInformation("EscalationJob tenant {Key}: saved {N} escalation log(s).", tenantKey, newLogs.Count);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AuditLog EscalationLog(int taskId, string action) => new()
    {
        EntityName = "GovernanceTask",
        EntityId   = taskId.ToString(),
        Action     = action,
        Username   = "system",
        Timestamp  = DateTime.UtcNow,
    };
}
