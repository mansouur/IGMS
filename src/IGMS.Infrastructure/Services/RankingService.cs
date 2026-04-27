using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

/// <summary>
/// يحسب درجات الأقسام والموظفين من البيانات الموجودة — لا جداول جديدة.
/// يطبّق مبدأ "Phantom Target": القسم/الموظف الأول يرى نفسه ثانياً.
/// </summary>
public class RankingService : IRankingService
{
    private readonly TenantDbContext _db;

    public RankingService(TenantDbContext db) => _db = db;

    // ── Phantom Target ────────────────────────────────────────────────────────
    private static int ApplyPhantomTarget(int trueRank, bool isAdmin)
        => (!isAdmin && trueRank == 1) ? 2 : trueRank;

    // ── DEPARTMENT RANKINGS ───────────────────────────────────────────────────

    public async Task<DepartmentRankingResponse> GetDepartmentRankingsAsync(
        int currentUserId, bool isAdmin)
    {
        var departments = await _db.Departments
            .Where(d => !d.IsDeleted)
            .Select(d => new { d.Id, d.NameAr })
            .ToListAsync();

        if (!departments.Any())
            return new DepartmentRankingResponse { IsAdminView = isAdmin };

        // معلومات المستخدم الحالي
        var currentUser = await _db.UserProfiles
            .Where(u => u.Id == currentUserId && !u.IsDeleted)
            .Select(u => new { u.Id, u.DepartmentId })
            .FirstOrDefaultAsync();

        // ── تجميع البيانات دفعة واحدة ─────────────────────────────────────

        // Tasks per department
        var tasks = await _db.Tasks
            .Where(t => !t.IsDeleted && t.DepartmentId != null)
            .GroupBy(t => t.DepartmentId!)
            .Select(g => new
            {
                DeptId = g.Key,
                Total  = g.Count(t => t.Status != Domain.Entities.TaskStatus.Cancelled),
                Done   = g.Count(t => t.Status == Domain.Entities.TaskStatus.Done),
            })
            .ToListAsync();

        // KPIs per department
        var kpis = await _db.Kpis
            .Where(k => !k.IsDeleted && k.DepartmentId != null)
            .GroupBy(k => k.DepartmentId!)
            .Select(g => new
            {
                DeptId  = g.Key,
                Total   = g.Count(),
                OnTrack = g.Count(k => k.Status == KpiStatus.OnTrack),
            })
            .ToListAsync();

        // Risks per department
        var risks = await _db.Risks
            .Where(r => !r.IsDeleted && r.DepartmentId != null)
            .GroupBy(r => r.DepartmentId!)
            .Select(g => new
            {
                DeptId    = g.Key,
                Total     = g.Count(),
                Addressed = g.Count(r => r.Status != RiskStatus.Open),
            })
            .ToListAsync();

        // Policy acknowledgment rate per department
        // الإقرار = عدد الأعضاء الذين أقروا بالسياسات النشطة / (عدد السياسات × عدد الأعضاء)
        var deptMembers = await _db.UserProfiles
            .Where(u => !u.IsDeleted && u.IsActive && u.DepartmentId != null)
            .GroupBy(u => u.DepartmentId!)
            .Select(g => new { DeptId = g.Key, MemberIds = g.Select(u => u.Id).ToList() })
            .ToListAsync();

        var activePolicies = await _db.Policies
            .Where(p => !p.IsDeleted && p.Status == PolicyStatus.Active && p.DepartmentId != null)
            .Select(p => new { p.Id, p.DepartmentId })
            .ToListAsync();

        var acknowledgments = await _db.PolicyAcknowledgments
            .Select(a => new { a.PolicyId, a.UserId })
            .ToListAsync();

        // Incidents per department
        var incidents = await _db.Incidents
            .Where(i => !i.IsDeleted && i.DepartmentId != null)
            .GroupBy(i => i.DepartmentId!)
            .Select(g => new
            {
                DeptId   = g.Key,
                Total    = g.Count(),
                Resolved = g.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed),
            })
            .ToListAsync();

        // Member counts per department
        var memberCounts = deptMembers
            .Where(d => d.DeptId.HasValue)
            .ToDictionary(d => d.DeptId!.Value, d => d.MemberIds.Count);

        // ── حساب درجة كل قسم ──────────────────────────────────────────────

        var scored = departments.Select(dept =>
        {
            // Tasks (20%)
            var t    = tasks.FirstOrDefault(x => x.DeptId == dept.Id);
            var tScore = t?.Total > 0 ? (decimal)t.Done / t.Total * 100m : 50m; // 50 عند غياب البيانات

            // KPIs (25%)
            var k    = kpis.FirstOrDefault(x => x.DeptId == dept.Id);
            var kScore = k?.Total > 0 ? (decimal)k.OnTrack / k.Total * 100m : 50m;

            // Risks (20%)
            var r    = risks.FirstOrDefault(x => x.DeptId == dept.Id);
            var rScore = r?.Total > 0 ? (decimal)r.Addressed / r.Total * 100m : 50m;

            // Policies acknowledgment (20%)
            var deptPolicies = activePolicies.Where(p => p.DepartmentId == dept.Id).ToList();
            var members      = deptMembers.FirstOrDefault(d => d.DeptId == dept.Id)?.MemberIds ?? [];
            decimal pScore   = 50m;
            if (deptPolicies.Any() && members.Any())
            {
                var expected = deptPolicies.Count * members.Count;
                var actual   = acknowledgments
                    .Count(a => deptPolicies.Any(p => p.Id == a.PolicyId) && members.Contains(a.UserId));
                pScore = (decimal)actual / expected * 100m;
            }

            // Incidents (15%)
            var i    = incidents.FirstOrDefault(x => x.DeptId == dept.Id);
            var iScore = i?.Total > 0 ? (decimal)i.Resolved / i.Total * 100m : 100m; // لا حوادث = ممتاز

            // الدرجة الإجمالية (أوزان محددة)
            var overall = Math.Round(
                tScore * 0.20m +
                kScore * 0.25m +
                rScore * 0.20m +
                pScore * 0.20m +
                iScore * 0.15m, 1);

            return new DepartmentRankDto
            {
                DepartmentId      = dept.Id,
                DepartmentNameAr  = dept.NameAr,
                OverallScore      = overall,
                TasksScore        = Math.Round(tScore, 1),
                KpisScore         = Math.Round(kScore, 1),
                RisksScore        = Math.Round(rScore, 1),
                PoliciesScore     = Math.Round(pScore, 1),
                IncidentsScore    = Math.Round(iScore, 1),
                MemberCount       = memberCounts.TryGetValue(dept.Id, out var mc) ? mc : 0,
                IsCurrentUserDept = currentUser?.DepartmentId == dept.Id,
            };
        })
        .OrderByDescending(d => d.OverallScore)
        .ThenBy(d => d.DepartmentId)
        .ToList();

        // تعيين الترتيب الحقيقي
        int totalDepts = scored.Count;
        for (int idx = 0; idx < scored.Count; idx++)
        {
            scored[idx].TrueRank        = idx + 1;
            scored[idx].DisplayRank     = ApplyPhantomTarget(idx + 1, isAdmin);
            scored[idx].TotalDepartments = totalDepts;
        }

        // الأدمن يرى الجميع — غيره يرى قسمه فقط
        var result = isAdmin
            ? scored
            : scored.Where(d => d.IsCurrentUserDept).ToList();

        return new DepartmentRankingResponse { Rankings = result, IsAdminView = isAdmin };
    }

    // ── EMPLOYEE RANKINGS ─────────────────────────────────────────────────────

    public async Task<EmployeeRankingResponse> GetEmployeeRankingsAsync(
        int currentUserId, bool isAdmin, bool isDeptManager)
    {
        var currentUser = await _db.UserProfiles
            .Where(u => u.Id == currentUserId && !u.IsDeleted)
            .Select(u => new { u.Id, u.DepartmentId, u.FullNameAr })
            .FirstOrDefaultAsync();

        if (currentUser is null)
            return new EmployeeRankingResponse();

        // نطاق الموظفين
        IQueryable<UserProfile> scope = _db.UserProfiles.Where(u => !u.IsDeleted && u.IsActive);

        string scopeLabel;
        if (isAdmin)
        {
            scopeLabel = "جميع الموظفين";
            // لا تصفية
        }
        else if (isDeptManager && currentUser.DepartmentId.HasValue)
        {
            scope      = scope.Where(u => u.DepartmentId == currentUser.DepartmentId);
            scopeLabel = "موظفو قسمك";
        }
        else
        {
            scope      = scope.Where(u => u.Id == currentUserId);
            scopeLabel = "درجتك الشخصية";
        }

        var users = await scope
            .Select(u => new
            {
                u.Id,
                u.FullNameAr,
                u.DepartmentId,
                DeptNameAr = u.Department != null ? u.Department.NameAr : "—",
            })
            .ToListAsync();

        if (!users.Any())
            return new EmployeeRankingResponse { IsAdminView = isAdmin, ScopeLabel = scopeLabel };

        var userIds = users.Select(u => u.Id).ToList();

        // ── تجميع البيانات ────────────────────────────────────────────────

        // Tasks
        var tasks = await _db.Tasks
            .Where(t => !t.IsDeleted && t.AssignedToId != null && userIds.Contains(t.AssignedToId!.Value))
            .GroupBy(t => t.AssignedToId!)
            .Select(g => new
            {
                UserId = g.Key,
                Total  = g.Count(t => t.Status != Domain.Entities.TaskStatus.Cancelled),
                Done   = g.Count(t => t.Status == Domain.Entities.TaskStatus.Done),
            })
            .ToListAsync();

        // Policy acknowledgments
        var activePolicyIds = await _db.Policies
            .Where(p => !p.IsDeleted && p.Status == PolicyStatus.Active)
            .Select(p => p.Id)
            .ToListAsync();

        var acks = await _db.PolicyAcknowledgments
            .Where(a => userIds.Contains(a.UserId) && activePolicyIds.Contains(a.PolicyId))
            .GroupBy(a => a.UserId)
            .Select(g => new { UserId = g.Key, AckedCount = g.Count() })
            .ToListAsync();

        int totalActivePolicies = activePolicyIds.Count;

        // Meeting action items
        var meetingActions = await _db.MeetingActionItems
            .Where(m => !m.IsDeleted && m.AssignedToId != null && userIds.Contains(m.AssignedToId!.Value))
            .GroupBy(m => m.AssignedToId!)
            .Select(g => new
            {
                UserId    = g.Key,
                Total     = g.Count(),
                Completed = g.Count(m => m.IsCompleted),
            })
            .ToListAsync();

        // Incidents reported by user
        var incidentsByUser = await _db.Incidents
            .Where(i => !i.IsDeleted && i.ReportedById != null && userIds.Contains(i.ReportedById!.Value))
            .GroupBy(i => i.ReportedById!)
            .Select(g => new
            {
                UserId   = g.Key,
                Total    = g.Count(),
                Resolved = g.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed),
            })
            .ToListAsync();

        // ── حساب درجة كل موظف ────────────────────────────────────────────

        var scored = users.Select(user =>
        {
            // Tasks (35%)
            var t      = tasks.FirstOrDefault(x => x.UserId == user.Id);
            var tScore = t?.Total > 0 ? (decimal)t.Done / t.Total * 100m : 50m;

            // Policies (25%)
            var a      = acks.FirstOrDefault(x => x.UserId == user.Id);
            var pScore = totalActivePolicies > 0
                ? Math.Min((decimal)(a?.AckedCount ?? 0) / totalActivePolicies * 100m, 100m)
                : 100m;

            // Meeting Actions (20%)
            var m      = meetingActions.FirstOrDefault(x => x.UserId == user.Id);
            var mScore = m?.Total > 0 ? (decimal)m.Completed / m.Total * 100m : 100m;

            // Incidents (20%)
            var i      = incidentsByUser.FirstOrDefault(x => x.UserId == user.Id);
            var iScore = i?.Total > 0 ? (decimal)i.Resolved / i.Total * 100m : 100m;

            var overall = Math.Round(
                tScore * 0.35m +
                pScore * 0.25m +
                mScore * 0.20m +
                iScore * 0.20m, 1);

            return new EmployeeRankDto
            {
                UserId              = user.Id,
                FullNameAr          = user.FullNameAr,
                DepartmentNameAr    = user.DeptNameAr,
                DepartmentId        = user.DepartmentId ?? 0,
                OverallScore        = overall,
                TasksScore          = Math.Round(tScore, 1),
                PoliciesScore       = Math.Round(pScore, 1),
                MeetingActionsScore = Math.Round(mScore, 1),
                IncidentsScore      = Math.Round(iScore, 1),
                IsCurrentUser       = user.Id == currentUserId,
            };
        })
        .OrderByDescending(e => e.OverallScore)
        .ThenBy(e => e.UserId)
        .ToList();

        int total = scored.Count;
        for (int idx = 0; idx < scored.Count; idx++)
        {
            scored[idx].TrueRank     = idx + 1;
            scored[idx].DisplayRank  = ApplyPhantomTarget(idx + 1, isAdmin);
            scored[idx].TotalInScope = total;
        }

        return new EmployeeRankingResponse
        {
            Rankings    = scored,
            IsAdminView = isAdmin,
            ScopeLabel  = scopeLabel,
        };
    }

    // ── MY SCORE ──────────────────────────────────────────────────────────────

    public async Task<MyScoreDto> GetMyScoreAsync(int currentUserId)
    {
        var deptRankings = await GetDepartmentRankingsAsync(currentUserId, isAdmin: false);
        var empRankings  = await GetEmployeeRankingsAsync(currentUserId, isAdmin: false, isDeptManager: false);

        var myDept = deptRankings.Rankings.FirstOrDefault(d => d.IsCurrentUserDept);
        var myEmp  = empRankings.Rankings.FirstOrDefault(e => e.IsCurrentUser);

        // تفاصيل الموظف
        var activePolicyIds = await _db.Policies
            .Where(p => !p.IsDeleted && p.Status == PolicyStatus.Active)
            .Select(p => p.Id).ToListAsync();

        var tasks = await _db.Tasks
            .Where(t => !t.IsDeleted && t.AssignedToId == currentUserId
                        && t.Status != Domain.Entities.TaskStatus.Cancelled)
            .ToListAsync();

        var acks = await _db.PolicyAcknowledgments
            .Where(a => a.UserId == currentUserId && activePolicyIds.Contains(a.PolicyId))
            .CountAsync();

        var meetingActions = await _db.MeetingActionItems
            .Where(m => !m.IsDeleted && m.AssignedToId == currentUserId)
            .ToListAsync();

        var incidents = await _db.Incidents
            .Where(i => !i.IsDeleted && i.ReportedById == currentUserId)
            .ToListAsync();

        return new MyScoreDto
        {
            DepartmentRank        = myDept,
            EmployeeRank          = myEmp,
            TasksTotal            = tasks.Count,
            TasksDone             = tasks.Count(t => t.Status == Domain.Entities.TaskStatus.Done),
            PoliciesTotal         = activePolicyIds.Count,
            PoliciesAcked         = acks,
            MeetingActionsTotal   = meetingActions.Count,
            MeetingActionsDone    = meetingActions.Count(m => m.IsCompleted),
            IncidentsTotal        = incidents.Count,
            IncidentsResolved     = incidents.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed),
        };
    }
}
