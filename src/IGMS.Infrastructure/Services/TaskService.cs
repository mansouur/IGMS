using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Common;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TaskStatus   = IGMS.Domain.Entities.TaskStatus;
using TaskPriority = IGMS.Domain.Entities.TaskPriority;
using TenantContext = IGMS.Application.Common.Models.TenantContext;

namespace IGMS.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly TenantDbContext      _db;
    private readonly INotificationService _notify;
    private readonly TenantContext        _tenant;

    public TaskService(TenantDbContext db, INotificationService notify, TenantContext tenant)
    {
        _db     = db;
        _notify = notify;
        _tenant = tenant;
    }

    // ── Shared filter builder ─────────────────────────────────────────────────
    private IQueryable<GovernanceTask> BuildQuery(TaskQuery q)
    {
        var query = _db.Tasks
            .Include(t => t.AssignedTo).Include(t => t.Department).Include(t => t.Risk)
            .Where(t => !t.IsDeleted).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(t => t.TitleAr.Contains(q.Search));
        if (q.Status.HasValue)       query = query.Where(t => t.Status       == q.Status.Value);
        if (q.Priority.HasValue)     query = query.Where(t => t.Priority     == q.Priority.Value);
        if (q.AssignedToId.HasValue) query = query.Where(t => t.AssignedToId == q.AssignedToId.Value);
        if (q.RiskId.HasValue)       query = query.Where(t => t.RiskId       == q.RiskId.Value);

        return query;
    }

    // ── Paged list ────────────────────────────────────────────────────────────
    public async Task<Result<PagedResult<TaskListDto>>> GetPagedAsync(TaskQuery q)
    {
        var query = BuildQuery(q);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(ToListDto)
            .ToListAsync();

        return Result<PagedResult<TaskListDto>>.Success(
            PagedResult<TaskListDto>.Create(items, total, q.Page, q.PageSize));
    }

    // ── Export (no pagination) ────────────────────────────────────────────────
    public async Task<byte[]> ExportAsync(TaskQuery q)
    {
        var items = await BuildQuery(q)
            .OrderByDescending(t => t.CreatedAt)
            .Select(ToListDto)
            .ToListAsync();

        var headers = new[]
        {
            "العنوان", "الحالة", "الأولوية", "الموعد النهائي",
            "المكلف", "القسم", "تاريخ الإنشاء",
        };

        string StatusLabel(TaskStatus s) => s switch
        {
            TaskStatus.Todo       => "قيد الانتظار",
            TaskStatus.InProgress => "جارية",
            TaskStatus.Done       => "منجزة",
            TaskStatus.Cancelled  => "ملغاة",
            _                     => s.ToString(),
        };

        string PriLabel(TaskPriority p) => p switch
        {
            TaskPriority.Low      => "منخفضة",
            TaskPriority.Medium   => "متوسطة",
            TaskPriority.High     => "عالية",
            TaskPriority.Critical => "حرجة",
            _                     => p.ToString(),
        };

        var rows = items.Select(t => new object?[]
        {
            t.TitleAr, StatusLabel(t.Status), PriLabel(t.Priority),
            t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy-MM-dd") : null,
            t.AssignedToNameAr, t.DepartmentNameAr,
            t.CreatedAt.ToString("yyyy-MM-dd"),
        });

        return ExcelExporter.Build("المهام", headers, rows);
    }

    // ── GetByRisk ─────────────────────────────────────────────────────────────
    public async Task<List<TaskListDto>> GetByRiskAsync(int riskId) =>
        await _db.Tasks
            .Include(t => t.AssignedTo).Include(t => t.Department).Include(t => t.Risk)
            .Where(t => !t.IsDeleted && t.RiskId == riskId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(ToListDto)
            .ToListAsync();

    // ── GetById ───────────────────────────────────────────────────────────────
    public async Task<Result<TaskDetailDto>> GetByIdAsync(int id)
    {
        var t = await _db.Tasks
            .Include(x => x.AssignedTo).Include(x => x.Department).Include(x => x.Risk)
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (t is null) return Result<TaskDetailDto>.Failure("المهمة غير موجودة.");
        return Result<TaskDetailDto>.Success(MapDetail(t));
    }

    // ── Save ──────────────────────────────────────────────────────────────────
    public async Task<Result<TaskDetailDto>> SaveAsync(SaveTaskRequest req, string by)
    {
        GovernanceTask task;
        var isNew = req.Id == 0;
        int? prevAssignee = null;

        if (isNew)
        {
            task = new GovernanceTask { CreatedAt = DateTime.UtcNow, CreatedBy = by };
            _db.Tasks.Add(task);
        }
        else
        {
            task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == req.Id && !t.IsDeleted)
                   ?? throw new KeyNotFoundException();
            prevAssignee        = task.AssignedToId;
            task.ModifiedAt = DateTime.UtcNow; task.ModifiedBy = by;
        }

        task.TitleAr = req.TitleAr; task.TitleEn = req.TitleEn;
        task.DescriptionAr = req.DescriptionAr;
        task.Status = req.Status; task.Priority = req.Priority;
        task.DueDate = req.DueDate;
        task.AssignedToId = req.AssignedToId; task.DepartmentId = req.DepartmentId;
        task.RiskId = req.RiskId;

        await _db.SaveChangesAsync();

        // Notify assignee only when it's a new task OR when the assignee changes
        bool assigneeChanged = !isNew && req.AssignedToId.HasValue && req.AssignedToId != prevAssignee;
        if ((isNew || assigneeChanged) && req.AssignedToId.HasValue)
        {
            var assignee = await _db.UserProfiles.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == req.AssignedToId.Value && !u.IsDeleted);

            if (assignee is not null)
            {
                await _notify.TaskAssignedAsync(
                    taskTitle:        req.TitleAr,
                    assigneeEmail:    assignee.Email,
                    assigneeName:     assignee.FullNameAr,
                    dueDate:          req.DueDate.HasValue ? req.DueDate.Value.ToString("yyyy-MM-dd") : null,
                    organizationName: _tenant.Organization.NameAr);
            }
        }

        return await GetByIdAsync(task.Id);
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    public async Task<Result<bool>> DeleteAsync(int id, string by)
    {
        var t = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (t is null) return Result<bool>.Failure("المهمة غير موجودة.");
        t.IsDeleted = true; t.ModifiedAt = DateTime.UtcNow; t.ModifiedBy = by;
        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Projections ───────────────────────────────────────────────────────────
    private static readonly System.Linq.Expressions.Expression<Func<GovernanceTask, TaskListDto>> ToListDto = t =>
        new TaskListDto
        {
            Id = t.Id, TitleAr = t.TitleAr, TitleEn = t.TitleEn,
            Status = t.Status, Priority = t.Priority, DueDate = t.DueDate,
            AssignedToNameAr = t.AssignedTo != null ? t.AssignedTo.FullNameAr : null,
            DepartmentNameAr = t.Department != null ? t.Department.NameAr     : null,
            CreatedAt = t.CreatedAt,
            RiskId       = t.RiskId,
            RiskTitleAr  = t.Risk != null ? t.Risk.TitleAr : null,
        };

    private static TaskDetailDto MapDetail(GovernanceTask t) => new()
    {
        Id = t.Id, TitleAr = t.TitleAr, TitleEn = t.TitleEn,
        DescriptionAr = t.DescriptionAr,
        Status = t.Status, Priority = t.Priority, DueDate = t.DueDate,
        AssignedToId = t.AssignedToId, AssignedToNameAr = t.AssignedTo?.FullNameAr,
        DepartmentId = t.DepartmentId, DepartmentNameAr = t.Department?.NameAr,
        CreatedAt = t.CreatedAt,
        RiskId      = t.RiskId,
        RiskTitleAr = t.Risk?.TitleAr,
    };
}
