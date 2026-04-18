using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainTaskStatus = IGMS.Domain.Entities.TaskStatus;

namespace IGMS.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly TenantDbContext    _db;
    private readonly ICurrentUserService _cu;

    public NotificationsController(TenantDbContext db, ICurrentUserService cu)
    {
        _db = db;
        _cu = cu;
    }

    /// <summary>
    /// إشعارات المستخدم الحالي — مُشتقّة من البيانات الحية (لا جدول منفصل).
    /// تشمل: مهام متأخرة، سياسات تستحق إقراراً، سياسات تنتهي قريباً، مخاطر حرجة.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var now    = DateTime.UtcNow;
        var userId = int.TryParse(_cu.UserId, out var uid) ? uid : 0;
        var isAdmin = _cu.Roles.Contains("ADMIN") || _cu.Roles.Contains("MANAGER");

        var notifications = new List<object>();

        // ── 1. مهامي المتأخرة ─────────────────────────────────────────────────
        var overdueTasks = await _db.Tasks
            .Where(t => !t.IsDeleted
                && t.AssignedToId == userId
                && t.DueDate < now
                && t.Status != DomainTaskStatus.Done)
            .Select(t => new { t.Id, t.TitleAr, t.DueDate })
            .ToListAsync();

        foreach (var task in overdueTasks)
        {
            var days = (int)(now - task.DueDate!.Value).TotalDays;
            notifications.Add(new
            {
                Type     = "overdue_task",
                Severity = "high",
                TitleAr  = "مهمة متأخرة",
                BodyAr   = $"{task.TitleAr} — متأخرة {days} يوم",
                Link     = $"/tasks/{task.Id}",
                At       = task.DueDate,
            });
        }

        // ── 2. مهامي المستحقة خلال 3 أيام ───────────────────────────────────
        var dueSoonTasks = await _db.Tasks
            .Where(t => !t.IsDeleted
                && t.AssignedToId == userId
                && t.DueDate >= now
                && t.DueDate <= now.AddDays(3)
                && t.Status != DomainTaskStatus.Done)
            .Select(t => new { t.Id, t.TitleAr, t.DueDate })
            .ToListAsync();

        foreach (var task in dueSoonTasks)
        {
            var days = (int)(task.DueDate!.Value - now).TotalDays;
            notifications.Add(new
            {
                Type     = "task_due_soon",
                Severity = "medium",
                TitleAr  = "مهمة تستحق قريباً",
                BodyAr   = $"{task.TitleAr} — تستحق خلال {days} يوم",
                Link     = $"/tasks/{task.Id}",
                At       = task.DueDate,
            });
        }

        // ── 3. سياسات فعّالة لم أُقرّها بعد ─────────────────────────────────
        var acknowledgedPolicyIds = await _db.PolicyAcknowledgments
            .Where(a => a.UserId == userId)
            .Select(a => a.PolicyId)
            .ToListAsync();

        var pendingAck = await _db.Policies
            .Where(p => !p.IsDeleted
                && p.Status == IGMS.Domain.Entities.PolicyStatus.Active
                && !acknowledgedPolicyIds.Contains(p.Id))
            .Select(p => new { p.Id, p.TitleAr })
            .Take(5)
            .ToListAsync();

        foreach (var policy in pendingAck)
        {
            notifications.Add(new
            {
                Type     = "pending_acknowledgment",
                Severity = "medium",
                TitleAr  = "سياسة بانتظار إقرارك",
                BodyAr   = policy.TitleAr,
                Link     = $"/policies/{policy.Id}",
                At       = now,
            });
        }

        // ── 4. سياسات تنتهي خلال 30 يوماً (للمدير/ADMIN فقط) ───────────────
        if (isAdmin)
        {
            var expiringPolicies = await _db.Policies
                .Where(p => !p.IsDeleted
                    && p.Status == IGMS.Domain.Entities.PolicyStatus.Active
                    && p.ExpiryDate != null
                    && p.ExpiryDate > now
                    && p.ExpiryDate <= now.AddDays(30))
                .Select(p => new { p.Id, p.TitleAr, p.ExpiryDate })
                .Take(3)
                .ToListAsync();

            foreach (var policy in expiringPolicies)
            {
                var days = (int)(policy.ExpiryDate!.Value - now).TotalDays;
                notifications.Add(new
                {
                    Type     = "policy_expiring",
                    Severity = days <= 7 ? "high" : "low",
                    TitleAr  = "سياسة تنتهي قريباً",
                    BodyAr   = $"{policy.TitleAr} — تنتهي خلال {days} يوم",
                    Link     = $"/policies/{policy.Id}",
                    At       = policy.ExpiryDate,
                });
            }
        }

        // ── 5. مخاطر حرجة مفتوحة (للمدير/ADMIN فقط) ────────────────────────
        if (isAdmin)
        {
            var criticalRisks = await _db.Risks
                .Where(r => !r.IsDeleted
                    && r.Status == IGMS.Domain.Entities.RiskStatus.Open
                    && r.Likelihood * r.Impact >= 15)
                .Select(r => new { r.Id, r.TitleAr, r.Likelihood, r.Impact })
                .Take(3)
                .ToListAsync();

            foreach (var risk in criticalRisks)
            {
                notifications.Add(new
                {
                    Type     = "critical_risk",
                    Severity = "high",
                    TitleAr  = "مخاطرة حرجة مفتوحة",
                    BodyAr   = $"{risk.TitleAr} — الدرجة: {risk.Likelihood * risk.Impact}",
                    Link     = $"/risks/{risk.Id}",
                    At       = now,
                });
            }
        }

        // رتّب: الأعلى خطورة أولاً
        var ordered = notifications
            .OrderBy(n => n is IDictionary<string, object> d
                ? (d["Severity"]?.ToString() == "high" ? 0 : d["Severity"]?.ToString() == "medium" ? 1 : 2)
                : 2)
            .ToList();

        return Ok(ApiResponse<object>.Ok(new
        {
            Count         = ordered.Count,
            Unread        = ordered.Count,
            Notifications = ordered,
        }));
    }
}
