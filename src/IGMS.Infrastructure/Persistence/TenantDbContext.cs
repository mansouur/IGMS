using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext scoped per HTTP request.
/// Code First: all schema changes via EF Migrations only.
/// Arabic_CI_AS collation applied globally.
/// SaveChangesAsync automatically captures audit entries for all entity changes.
/// </summary>
public class TenantDbContext : DbContext, ITenantDbContext
{
    private readonly TenantContext         _tenantContext;
    private readonly ICurrentUserService?  _currentUser;
    private readonly IHttpContextAccessor? _http;

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        TenantContext tenantContext,
        ICurrentUserService? currentUser = null,
        IHttpContextAccessor? http = null)
        : base(options)
    {
        _tenantContext = tenantContext;
        _currentUser   = currentUser;
        _http          = http;
    }

    // ── DbSets ────────────────────────────────────────────────────────────────

    public DbSet<Department>      Departments      => Set<Department>();
    public DbSet<UserProfile>     UserProfiles     => Set<UserProfile>();
    public DbSet<Role>            Roles            => Set<Role>();
    public DbSet<Permission>      Permissions      => Set<Permission>();
    public DbSet<UserRole>        UserRoles        => Set<UserRole>();
    public DbSet<RolePermission>  RolePermissions  => Set<RolePermission>();
    public DbSet<AuditLog>        AuditLogs        => Set<AuditLog>();

    // ── RACI Module ───────────────────────────────────────────────────────────
    public DbSet<RaciMatrix>      RaciMatrices     => Set<RaciMatrix>();
    public DbSet<RaciActivity>    RaciActivities   => Set<RaciActivity>();
    public DbSet<RaciParticipant> RaciParticipants => Set<RaciParticipant>();

    // ── Governance Modules ────────────────────────────────────────────────────
    public DbSet<Policy>                 Policies               => Set<Policy>();
    public DbSet<PolicyAttachment>       PolicyAttachments      => Set<PolicyAttachment>();
    public DbSet<PolicyAcknowledgment>   PolicyAcknowledgments  => Set<PolicyAcknowledgment>();
    public DbSet<Risk>              Risks              => Set<Risk>();
    public DbSet<GovernanceTask>    Tasks              => Set<GovernanceTask>();
    public DbSet<Kpi>               Kpis               => Set<Kpi>();
    public DbSet<KpiRecord>         KpiRecords         => Set<KpiRecord>();
    public DbSet<ComplianceMapping> ComplianceMappings => Set<ComplianceMapping>();
    public DbSet<RiskKpiMapping>    RiskKpiMappings    => Set<RiskKpiMapping>();
    public DbSet<ControlTest>       ControlTests       => Set<ControlTest>();
    public DbSet<ControlEvidence>   ControlEvidences   => Set<ControlEvidence>();

    // ── Assessment / Survey ───────────────────────────────────────────────────
    public DbSet<Assessment>         Assessments        => Set<Assessment>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
    public DbSet<AssessmentResponse> AssessmentResponses => Set<AssessmentResponse>();
    public DbSet<AssessmentAnswer>   AssessmentAnswers   => Set<AssessmentAnswer>();

    // ── Incident Management ──────────────────────────────────────────────────
    public DbSet<Incident> Incidents => Set<Incident>();

    // ── Vendor Risk ───────────────────────────────────────────────────────────
    public DbSet<Vendor> Vendors => Set<Vendor>();

    // ── Meeting Management ────────────────────────────────────────────────────
    public DbSet<Meeting>           Meetings           => Set<Meeting>();
    public DbSet<MeetingAttendee>   MeetingAttendees   => Set<MeetingAttendee>();
    public DbSet<MeetingActionItem> MeetingActionItems => Set<MeetingActionItem>();

    // ── Performance Management (HPMS) ─────────────────────────────────────────
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<PerformanceGoal>   PerformanceGoals   => Set<PerformanceGoal>();

    // ── Regulatory Library ───────────────────────────────────────────────────
    public DbSet<RegulatoryFramework> RegulatoryFrameworks => Set<RegulatoryFramework>();
    public DbSet<RegulatoryControl>   RegulatoryControls   => Set<RegulatoryControl>();
    public DbSet<ControlMapping>      ControlMappings      => Set<ControlMapping>();

    // ── Workflow Engine ───────────────────────────────────────────────────────
    public DbSet<WorkflowDefinition>    WorkflowDefinitions    => Set<WorkflowDefinition>();
    public DbSet<WorkflowStage>         WorkflowStages         => Set<WorkflowStage>();
    public DbSet<WorkflowInstance>      WorkflowInstances      => Set<WorkflowInstance>();
    public DbSet<WorkflowInstanceAction> WorkflowInstanceActions => Set<WorkflowInstanceAction>();

    // ── Audit interception ────────────────────────────────────────────────────

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Capture pre-save state for all auditable entities
        var auditEntries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => new AuditEntry(e))
            .ToList();

        // 2. Persist the real changes
        var result = await base.SaveChangesAsync(cancellationToken);

        // 3. Post-save: resolve Added entity IDs (database-generated keys)
        foreach (var ae in auditEntries.Where(a => a.NeedsIdAfterSave))
            ae.Resolve();

        // 4. Write audit rows (if any changes happened)
        if (auditEntries.Count > 0)
        {
            var username = _currentUser?.Username ?? "system";
            var userId   = int.TryParse(_currentUser?.UserId, out var uid) ? uid : (int?)null;
            var ip       = _http?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var ua       = _http?.HttpContext?.Request.Headers.UserAgent.ToString();

            foreach (var ae in auditEntries)
            {
                AuditLogs.Add(new AuditLog
                {
                    EntityName = ae.EntityName,
                    EntityId   = ae.EntityId,
                    Action     = ae.Action,
                    OldValues  = ae.OldValues,
                    NewValues  = ae.NewValues,
                    UserId     = userId,
                    Username   = username,
                    IpAddress  = ip,
                    UserAgent  = ua,
                    Timestamp  = DateTime.UtcNow,
                });
            }

            // 5. Persist audit rows (bypass the override to avoid recursion)
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    // ── Configuration ─────────────────────────────────────────────────────────

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                _tenantContext.ConnectionString,
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Arabic_CI_AS");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
        DbSeeder.Seed(modelBuilder);
        RegulatorySeeder.Seed(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}
