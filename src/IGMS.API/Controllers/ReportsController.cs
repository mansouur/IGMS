using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using Microsoft.AspNetCore.RateLimiting;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainTaskStatus = IGMS.Domain.Entities.TaskStatus;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Produces("application/json")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly TenantDbContext _db;
    private readonly IExecutivePdfService _pdf;
    public ReportsController(TenantDbContext db, IExecutivePdfService pdf)
    { _db = db; _pdf = pdf; }

    /// <summary>ملخص إحصائي شامل مع مؤشرات صحة الحوكمة.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var now = DateTime.UtcNow;
        var in30 = now.AddDays(30);
        var in60 = now.AddDays(60);

        // ── Policies ──────────────────────────────────────────────────────────
        var polTotal    = await _db.Policies.CountAsync(p => !p.IsDeleted);
        var polActive   = await _db.Policies.CountAsync(p => !p.IsDeleted && p.Status == PolicyStatus.Active);
        var polDraft    = await _db.Policies.CountAsync(p => !p.IsDeleted && p.Status == PolicyStatus.Draft);
        var polArchived = await _db.Policies.CountAsync(p => !p.IsDeleted && p.Status == PolicyStatus.Archived);
        var polExp30    = await _db.Policies.CountAsync(p => !p.IsDeleted && p.Status == PolicyStatus.Active
                            && p.ExpiryDate.HasValue && p.ExpiryDate.Value <= in30 && p.ExpiryDate.Value >= now);
        var polExp60    = await _db.Policies.CountAsync(p => !p.IsDeleted && p.Status == PolicyStatus.Active
                            && p.ExpiryDate.HasValue && p.ExpiryDate.Value > in30 && p.ExpiryDate.Value <= in60);

        // ── Risks ─────────────────────────────────────────────────────────────
        var rskTotal     = await _db.Risks.CountAsync(r => !r.IsDeleted);
        var rskOpen      = await _db.Risks.CountAsync(r => !r.IsDeleted && r.Status == RiskStatus.Open);
        var rskMitigated = await _db.Risks.CountAsync(r => !r.IsDeleted && r.Status == RiskStatus.Mitigated);
        var rskClosed    = await _db.Risks.CountAsync(r => !r.IsDeleted && r.Status == RiskStatus.Closed);
        var rskHighRisk  = await _db.Risks.CountAsync(r => !r.IsDeleted && r.Likelihood * r.Impact >= 15);

        // ── Tasks ─────────────────────────────────────────────────────────────
        var tskTotal      = await _db.Tasks.CountAsync(t => !t.IsDeleted);
        var tskTodo       = await _db.Tasks.CountAsync(t => !t.IsDeleted && t.Status == DomainTaskStatus.Todo);
        var tskInProgress = await _db.Tasks.CountAsync(t => !t.IsDeleted && t.Status == DomainTaskStatus.InProgress);
        var tskDone       = await _db.Tasks.CountAsync(t => !t.IsDeleted && t.Status == DomainTaskStatus.Done);
        var tskOverdue    = await _db.Tasks.CountAsync(t => !t.IsDeleted
            && t.DueDate < now
            && t.Status != DomainTaskStatus.Done
            && t.Status != DomainTaskStatus.Cancelled);

        // ── KPIs ──────────────────────────────────────────────────────────────
        var kpiTotal   = await _db.Kpis.CountAsync(k => !k.IsDeleted);
        var kpiOnTrack = await _db.Kpis.CountAsync(k => !k.IsDeleted && k.Status == KpiStatus.OnTrack);
        var kpiAtRisk  = await _db.Kpis.CountAsync(k => !k.IsDeleted && k.Status == KpiStatus.AtRisk);
        var kpiBehind  = await _db.Kpis.CountAsync(k => !k.IsDeleted && k.Status == KpiStatus.Behind);

        // KPI average achievement from actual records (last record per KPI) – computed in-memory
        var kpiRawRecords = await _db.KpiRecords
            .Where(r => !r.IsDeleted && r.TargetValue > 0)
            .Select(r => new { r.KpiId, r.Year, r.Quarter, r.ActualValue, r.TargetValue })
            .ToListAsync();

        double kpiAvgAchievement = 0;
        if (kpiRawRecords.Count > 0)
        {
            var lastPerKpi = kpiRawRecords
                .GroupBy(r => r.KpiId)
                .Select(g => g.OrderByDescending(r => r.Year).ThenByDescending(r => r.Quarter).First());
            kpiAvgAchievement = lastPerKpi.Average(r => (double)r.ActualValue / (double)r.TargetValue * 100);
        }

        // ── Compliance Coverage ───────────────────────────────────────────────
        var polWithCompliance = await _db.ComplianceMappings
            .Where(c => c.EntityType == "Policy")
            .Select(c => c.EntityId).Distinct().CountAsync();
        var rskWithCompliance = await _db.ComplianceMappings
            .Where(c => c.EntityType == "Risk")
            .Select(c => c.EntityId).Distinct().CountAsync();

        double polCompliancePct = polTotal > 0 ? Math.Round((double)polWithCompliance / polTotal * 100, 1) : 0;
        double rskCompliancePct = rskTotal > 0 ? Math.Round((double)rskWithCompliance / rskTotal * 100, 1) : 0;

        // ── Governance Health Score (0–100) ───────────────────────────────────
        // Policy health: % active of total (weight 25)
        double polHealth = polTotal > 0 ? (double)polActive / polTotal * 100 : 0;
        // Risk health: % mitigated+closed of total (weight 25)
        double rskHealth = rskTotal > 0 ? (double)(rskMitigated + rskClosed) / rskTotal * 100 : 100;
        // Task health: % done of (done+overdue+inProgress) – penalise overdue (weight 25)
        double tskBase   = tskTotal > 0 ? (double)tskDone / tskTotal * 100 : 100;
        double tskPenalty = tskTotal > 0 ? (double)tskOverdue / tskTotal * 50 : 0;
        double tskHealth = Math.Max(0, tskBase - tskPenalty);
        // KPI health: % onTrack of total (weight 25)
        double kpiHealth = kpiTotal > 0 ? (double)kpiOnTrack / kpiTotal * 100 : 100;

        int governanceScore = (int)Math.Round((polHealth + rskHealth + tskHealth + kpiHealth) / 4);

        // ── Users / Departments ───────────────────────────────────────────────
        var usrTotal  = await _db.UserProfiles.CountAsync(u => !u.IsDeleted);
        var usrActive = await _db.UserProfiles.CountAsync(u => !u.IsDeleted && u.IsActive);
        var deptTotal = await _db.Departments.CountAsync(d => !d.IsDeleted);

        var summary = new
        {
            Policies = new
            {
                Total    = polTotal,   Active   = polActive,
                Draft    = polDraft,   Archived = polArchived,
                ExpiringIn30Days = polExp30, ExpiringIn60Days = polExp60,
            },
            Risks = new
            {
                Total     = rskTotal,   Open      = rskOpen,
                Mitigated = rskMitigated, Closed  = rskClosed,
                HighRisk  = rskHighRisk,
            },
            Tasks = new
            {
                Total      = tskTotal,      Todo       = tskTodo,
                InProgress = tskInProgress, Done       = tskDone,
                Overdue    = tskOverdue,
            },
            Kpis = new
            {
                Total   = kpiTotal,   OnTrack = kpiOnTrack,
                AtRisk  = kpiAtRisk,  Behind  = kpiBehind,
                AvgAchievement = Math.Round(kpiAvgAchievement, 1),
            },
            Compliance = new
            {
                PoliciesCovered    = polWithCompliance,
                PoliciesCoveredPct = polCompliancePct,
                RisksCovered       = rskWithCompliance,
                RisksCoveredPct    = rskCompliancePct,
            },
            GovernanceScore = governanceScore,
            Users       = new { Total = usrTotal,  Active = usrActive },
            Departments = new { Total = deptTotal },
        };

        return Ok(ApiResponse<object>.Ok(summary));
    }

    /// <summary>أعلى 5 مخاطر حرجة مفتوحة.</summary>
    [HttpGet("top-risks")]
    public async Task<IActionResult> GetTopRisks()
    {
        var risks = await _db.Risks
            .Where(r => !r.IsDeleted && r.Status == RiskStatus.Open)
            .OrderByDescending(r => r.Likelihood * r.Impact)
            .Take(5)
            .Select(r => new
            {
                r.Id, r.TitleAr, r.Code,
                Score    = r.Likelihood * r.Impact,
                r.Likelihood, r.Impact,
                Category = r.Category.ToString(),
                DepartmentNameAr = r.Department != null ? r.Department.NameAr : null,
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(risks));
    }

    /// <summary>بطاقة أداء الأقسام – مؤشرات KPI مجمّعة حسب القسم لسنة معيّنة.</summary>
    [HttpGet("department-scorecard")]
    public async Task<IActionResult> GetDepartmentScorecard([FromQuery] int? year)
    {
        var y = year ?? DateTime.UtcNow.Year;

        // Materialise to memory – avoid EF GroupBy translation issues
        var kpis = await _db.Kpis
            .Include(k => k.Department)
            .Where(k => !k.IsDeleted && k.Year == y)
            .AsNoTracking()
            .ToListAsync();

        var scorecard = kpis
            .GroupBy(k => new { k.DepartmentId, Name = k.Department?.NameAr ?? "غير محدد" })
            .Select(g =>
            {
                var items = g.Select(k =>
                {
                    double pct = k.TargetValue == 0 ? 0 : (double)k.ActualValue / (double)k.TargetValue * 100;
                    return new KpiScorecardItemDto
                    {
                        Id             = k.Id,
                        TitleAr        = k.TitleAr,
                        Code           = k.Code,
                        AchievementPct = Math.Round(Math.Clamp(pct, 0, 150), 1),
                        Status         = k.Status,
                    };
                }).OrderBy(x => x.AchievementPct).ToList();

                double avg   = items.Count > 0 ? items.Average(x => Math.Min(x.AchievementPct, 100)) : 0;
                int    score = (int)Math.Round(Math.Clamp(avg, 0, 100));

                return new DepartmentScorecardDto
                {
                    DepartmentId      = g.Key.DepartmentId,
                    DepartmentNameAr  = g.Key.Name,
                    KpiCount          = items.Count,
                    OnTrackCount      = g.Count(k => k.Status == KpiStatus.OnTrack),
                    AtRiskCount       = g.Count(k => k.Status == KpiStatus.AtRisk),
                    BehindCount       = g.Count(k => k.Status == KpiStatus.Behind),
                    AvgAchievementPct = Math.Round(avg, 1),
                    Score             = score,
                    Kpis              = items,
                };
            })
            .OrderByDescending(d => d.Score)
            .ToList();

        return Ok(ApiResponse<List<DepartmentScorecardDto>>.Ok(scorecard));
    }

    /// <summary>التقرير التنفيذي بصيغة PDF.</summary>
    [EnableRateLimiting("export")]
    [HttpGet("executive-pdf")]
    public async Task<IActionResult> GetExecutivePdf([FromQuery] string? orgName)
    {
        var name  = orgName ?? "المؤسسة";
        var bytes = await _pdf.GenerateExecutiveReportAsync(name);
        var file  = $"executive_report_{DateTime.UtcNow:yyyyMMdd}.pdf";
        return File(bytes, "application/pdf", file);
    }

    /// <summary>بيانات تقرير الامتثال (للعرض والاختبار بدون انتظار الجدولة).</summary>
    [HttpGet("compliance-report")]
    public async Task<IActionResult> GetComplianceReport()
    {
        var data = await IGMS.Infrastructure.Jobs.ComplianceReportJob.BuildReportAsync(
            _db, DateTime.UtcNow, "", default);
        return Ok(ApiResponse<object>.Ok(data));
    }

    /// <summary>آخر 10 إجراءات في سجل المراجعة.</summary>
    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetRecentActivity()
    {
        var activity = await _db.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new
            {
                a.EntityName, a.Action, a.Username, a.Timestamp,
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(activity));
    }

    /// <summary>
    /// اتجاه إنجاز مؤشرات الأداء عبر الأرباع — مجمَّع من KpiRecords.
    /// يُعيد متوسط نسبة الإنجاز لكل ربع سنوي مرتبةً تصاعدياً.
    /// </summary>
    [HttpGet("kpi-trend")]
    public async Task<IActionResult> GetKpiTrend()
    {
        var records = await _db.KpiRecords
            .Where(r => !r.IsDeleted && r.TargetValue > 0)
            .Select(r => new { r.Year, r.Quarter, r.ActualValue, r.TargetValue })
            .ToListAsync();

        var trend = records
            .GroupBy(r => new { r.Year, r.Quarter })
            .Select(g =>
            {
                var avg = g.Average(r => Math.Min((double)r.ActualValue / (double)r.TargetValue * 100, 150));
                return new
                {
                    Year         = g.Key.Year,
                    Quarter      = g.Key.Quarter,
                    Period       = $"Q{g.Key.Quarter} {g.Key.Year}",
                    AvgAchievement = Math.Round(Math.Min(avg, 100), 1),
                    KpiCount     = g.Count(),
                };
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Quarter)
            .ToList();

        return Ok(ApiResponse<object>.Ok(trend));
    }
}
