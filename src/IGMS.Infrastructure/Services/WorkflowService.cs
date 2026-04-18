using IGMS.Application.Common.Interfaces;
using IGMS.Application.Common.Models;
using IGMS.Domain.Entities;
using IGMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Services;

/// <summary>
/// Implements multi-stage approval workflows per entity type.
/// Flow: Submit → stage 1 pending → approver acts → stage 2 pending … → Approved
/// Rejection at any stage returns the instance to Rejected status immediately.
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly TenantDbContext _db;

    public WorkflowService(TenantDbContext db) => _db = db;

    // ─────────────────────────────── DEFINITIONS ─────────────────────────────

    public async Task<List<WorkflowDefinitionListDto>> GetDefinitionsAsync(string? entityType = null)
    {
        var q = _db.WorkflowDefinitions
            .AsNoTracking()
            .Where(d => !d.IsDeleted);

        if (!string.IsNullOrEmpty(entityType))
            q = q.Where(d => d.EntityType == entityType);

        return await q
            .OrderBy(d => d.EntityType).ThenBy(d => d.NameAr)
            .Select(d => new WorkflowDefinitionListDto
            {
                Id         = d.Id,
                EntityType = d.EntityType,
                NameAr     = d.NameAr,
                NameEn     = d.NameEn,
                IsActive   = d.IsActive,
                StageCount = d.Stages.Count(s => !s.IsDeleted),
            })
            .ToListAsync();
    }

    public async Task<WorkflowDefinitionDetailDto?> GetDefinitionByIdAsync(int id)
    {
        var d = await _db.WorkflowDefinitions
            .AsNoTracking()
            .Include(x => x.Stages.Where(s => !s.IsDeleted).OrderBy(s => s.StageOrder))
                .ThenInclude(s => s.RequiredRole)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (d == null) return null;

        return MapDetail(d);
    }

    public async Task<WorkflowDefinitionDetailDto> SaveDefinitionAsync(int? id, SaveWorkflowDefinitionRequest req)
    {
        WorkflowDefinition def;

        if (id.HasValue)
        {
            def = await _db.WorkflowDefinitions
                .Include(x => x.Stages.Where(s => !s.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted)
                ?? throw new InvalidOperationException("تعريف سير العمل غير موجود.");

            def.EntityType    = req.EntityType;
            def.NameAr        = req.NameAr;
            def.NameEn        = req.NameEn;
            def.DescriptionAr = req.DescriptionAr;
            def.IsActive      = req.IsActive;
            def.ModifiedAt    = DateTime.UtcNow;

            // Remove old stages and rebuild
            foreach (var s in def.Stages.ToList())
            {
                s.IsDeleted = true;
                s.DeletedAt = DateTime.UtcNow;
            }
        }
        else
        {
            def = new WorkflowDefinition
            {
                EntityType    = req.EntityType,
                NameAr        = req.NameAr,
                NameEn        = req.NameEn,
                DescriptionAr = req.DescriptionAr,
                IsActive      = req.IsActive,
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = "api",
            };
            _db.WorkflowDefinitions.Add(def);
        }

        // Add new stages
        for (int i = 0; i < req.Stages.Count; i++)
        {
            var s = req.Stages[i];
            def.Stages.Add(new WorkflowStage
            {
                StageOrder     = i + 1,
                NameAr         = s.NameAr,
                NameEn         = s.NameEn,
                RequiredRoleId = s.RequiredRoleId,
                CreatedAt      = DateTime.UtcNow,
                CreatedBy      = "api",
            });
        }

        await _db.SaveChangesAsync();

        return MapDetail(def);
    }

    public async Task DeleteDefinitionAsync(int id)
    {
        var def = await _db.WorkflowDefinitions.FindAsync(id)
            ?? throw new InvalidOperationException("تعريف سير العمل غير موجود.");

        var hasActiveInstances = await _db.WorkflowInstances
            .AnyAsync(i => i.WorkflowDefinitionId == id && i.Status == WorkflowStatus.Pending && !i.IsDeleted);

        if (hasActiveInstances)
            throw new InvalidOperationException("لا يمكن حذف تعريف له دورات اعتماد نشطة.");

        def.IsDeleted = true;
        def.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ─────────────────────────────── INSTANCES ───────────────────────────────

    public async Task<WorkflowInstanceDto?> SubmitAsync(SubmitWorkflowRequest req, int submittedById)
    {
        // Find the active definition for this entity type
        var def = await _db.WorkflowDefinitions
            .AsNoTracking()
            .Include(d => d.Stages.Where(s => !s.IsDeleted).OrderBy(s => s.StageOrder))
            .FirstOrDefaultAsync(d => d.EntityType == req.EntityType && d.IsActive && !d.IsDeleted);

        if (def == null || !def.Stages.Any())
            throw new InvalidOperationException("لا يوجد تعريف سير عمل نشط لهذا النوع من العناصر.");

        // Cancel any existing pending instance for this entity
        var existing = await _db.WorkflowInstances
            .Where(i => i.EntityType == req.EntityType && i.EntityId == req.EntityId
                        && i.Status == WorkflowStatus.Pending && !i.IsDeleted)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
        }

        var firstStage = def.Stages.OrderBy(s => s.StageOrder).First();

        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = def.Id,
            EntityType           = req.EntityType,
            EntityId             = req.EntityId,
            Status               = WorkflowStatus.Pending,
            CurrentStageOrder    = firstStage.StageOrder,
            SubmittedById        = submittedById,
            CreatedAt            = DateTime.UtcNow,
            CreatedBy            = "api",
        };

        _db.WorkflowInstances.Add(instance);
        await _db.SaveChangesAsync();

        return await GetInstanceAsync(req.EntityType, req.EntityId);
    }

    public async Task<WorkflowInstanceDto?> GetInstanceAsync(string entityType, int entityId)
    {
        var instance = await _db.WorkflowInstances
            .AsNoTracking()
            .Include(i => i.Definition)
                .ThenInclude(d => d!.Stages.Where(s => !s.IsDeleted).OrderBy(s => s.StageOrder))
            .Include(i => i.SubmittedBy)
            .Include(i => i.Actions)
                .ThenInclude(a => a.Actor)
            .Where(i => i.EntityType == entityType && i.EntityId == entityId && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync();

        if (instance == null) return null;

        return MapInstance(instance, actorId: null);
    }

    public async Task<List<PendingApprovalDto>> GetPendingAsync(int userId)
    {
        // Get all roles the user belongs to
        var userRoleIds = await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        // Get pending instances where the current stage's required role matches
        var instances = await _db.WorkflowInstances
            .AsNoTracking()
            .Include(i => i.Definition)
                .ThenInclude(d => d!.Stages.Where(s => !s.IsDeleted).OrderBy(s => s.StageOrder))
            .Include(i => i.SubmittedBy)
            .Where(i => i.Status == WorkflowStatus.Pending && !i.IsDeleted)
            .ToListAsync();

        var pending = new List<PendingApprovalDto>();

        foreach (var inst in instances)
        {
            var currentStage = inst.Definition?.Stages
                .FirstOrDefault(s => s.StageOrder == inst.CurrentStageOrder);

            if (currentStage == null) continue;

            // Anyone can act if no role required; otherwise must belong to required role
            bool canAct = !currentStage.RequiredRoleId.HasValue
                          || userRoleIds.Contains(currentStage.RequiredRoleId.Value);

            if (!canAct) continue;

            // Skip if already acted on this stage
            var alreadyActed = await _db.WorkflowInstanceActions
                .AnyAsync(a => a.WorkflowInstanceId == inst.Id
                               && a.StageOrder == inst.CurrentStageOrder
                               && a.ActorId == userId);

            if (alreadyActed) continue;

            var entityTitle = await GetEntityTitleAsync(inst.EntityType, inst.EntityId);

            pending.Add(new PendingApprovalDto
            {
                InstanceId   = inst.Id,
                EntityType   = inst.EntityType,
                EntityId     = inst.EntityId,
                EntityTitle  = entityTitle,
                WorkflowName = inst.Definition?.NameAr ?? "",
                CurrentStage = currentStage.NameAr,
                SubmittedBy  = inst.SubmittedBy?.FullNameAr ?? "",
                SubmittedAt  = inst.CreatedAt,
            });
        }

        return pending;
    }

    public async Task<WorkflowInstanceDto?> ActAsync(int instanceId, ActOnWorkflowRequest req, int actorId)
    {
        var instance = await _db.WorkflowInstances
            .Include(i => i.Definition)
                .ThenInclude(d => d!.Stages.Where(s => !s.IsDeleted).OrderBy(s => s.StageOrder))
            .FirstOrDefaultAsync(i => i.Id == instanceId && !i.IsDeleted)
            ?? throw new InvalidOperationException("دورة الاعتماد غير موجودة.");

        if (instance.Status != WorkflowStatus.Pending)
            throw new InvalidOperationException("دورة الاعتماد مكتملة بالفعل.");

        if (!Enum.TryParse<WorkflowDecision>(req.Decision, ignoreCase: true, out var decision))
            throw new InvalidOperationException("قرار غير صالح.");

        var currentStage = instance.Definition!.Stages
            .FirstOrDefault(s => s.StageOrder == instance.CurrentStageOrder)
            ?? throw new InvalidOperationException("المرحلة الحالية غير موجودة.");

        // Log the action
        _db.WorkflowInstanceActions.Add(new WorkflowInstanceAction
        {
            WorkflowInstanceId = instanceId,
            StageOrder         = currentStage.StageOrder,
            ActorId            = actorId,
            Decision           = decision,
            Comment            = req.Comment,
            ActedAt            = DateTime.UtcNow,
            CreatedAt          = DateTime.UtcNow,
            CreatedBy          = "api",
        });

        if (decision == WorkflowDecision.Rejected)
        {
            instance.Status            = WorkflowStatus.Rejected;
            instance.CurrentStageOrder = null;
        }
        else if (decision == WorkflowDecision.Approved)
        {
            // Move to next stage or complete
            var nextStage = instance.Definition.Stages
                .OrderBy(s => s.StageOrder)
                .FirstOrDefault(s => s.StageOrder > currentStage.StageOrder);

            if (nextStage != null)
            {
                instance.CurrentStageOrder = nextStage.StageOrder;
            }
            else
            {
                instance.Status            = WorkflowStatus.Approved;
                instance.CurrentStageOrder = null;
            }
        }
        // Commented: stage stays the same

        await _db.SaveChangesAsync();

        return await GetInstanceAsync(instance.EntityType, instance.EntityId);
    }

    // ─────────────────────────────── Helpers ─────────────────────────────────

    private static WorkflowDefinitionDetailDto MapDetail(WorkflowDefinition d) => new()
    {
        Id            = d.Id,
        EntityType    = d.EntityType,
        NameAr        = d.NameAr,
        NameEn        = d.NameEn,
        DescriptionAr = d.DescriptionAr,
        IsActive      = d.IsActive,
        Stages        = d.Stages
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.StageOrder)
            .Select(s => new WorkflowStageDto
            {
                Id                 = s.Id,
                StageOrder         = s.StageOrder,
                NameAr             = s.NameAr,
                NameEn             = s.NameEn,
                RequiredRoleId     = s.RequiredRoleId,
                RequiredRoleNameAr = s.RequiredRole?.NameAr,
            }).ToList(),
    };

    private static WorkflowInstanceDto MapInstance(WorkflowInstance i, int? actorId)
    {
        var stages = i.Definition?.Stages ?? [];
        var currentStage = stages.FirstOrDefault(s => s.StageOrder == i.CurrentStageOrder);

        return new WorkflowInstanceDto
        {
            Id                  = i.Id,
            Status              = i.Status.ToString(),
            CurrentStageOrder   = i.CurrentStageOrder,
            CurrentStageNameAr  = currentStage?.NameAr,
            DefinitionNameAr    = i.Definition?.NameAr ?? "",
            SubmittedByName     = i.SubmittedBy?.FullNameAr ?? "",
            SubmittedAt         = i.CreatedAt,
            CanAct              = false, // set by controller after role check
            Actions             = i.Actions
                .OrderBy(a => a.ActedAt)
                .Select(a => new WorkflowActionDto
                {
                    Id          = a.Id,
                    StageOrder  = a.StageOrder,
                    StageNameAr = stages.FirstOrDefault(s => s.StageOrder == a.StageOrder)?.NameAr ?? $"مرحلة {a.StageOrder}",
                    ActorName   = a.Actor?.FullNameAr ?? "",
                    Decision    = a.Decision.ToString(),
                    Comment     = a.Comment,
                    ActedAt     = a.ActedAt,
                }).ToList(),
        };
    }

    private async Task<string> GetEntityTitleAsync(string entityType, int entityId)
    {
        return entityType switch
        {
            "Policy"      => (await _db.Policies.AsNoTracking().Where(p => p.Id == entityId).Select(p => p.TitleAr).FirstOrDefaultAsync()) ?? $"سياسة #{entityId}",
            "Risk"        => (await _db.Risks.AsNoTracking().Where(r => r.Id == entityId).Select(r => r.TitleAr).FirstOrDefaultAsync()) ?? $"مخاطرة #{entityId}",
            "ControlTest" => (await _db.ControlTests.AsNoTracking().Where(c => c.Id == entityId).Select(c => c.TitleAr).FirstOrDefaultAsync()) ?? $"ضابط #{entityId}",
            _             => $"{entityType} #{entityId}",
        };
    }
}
