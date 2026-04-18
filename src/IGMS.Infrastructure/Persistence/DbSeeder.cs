using IGMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IGMS.Infrastructure.Persistence;

/// <summary>
/// Seed data applied via EF Migrations (HasData).
/// Runs once on InitialCreate – never modified manually.
/// To add new permissions: add here + create a new migration.
/// </summary>
internal static class DbSeeder
{
    private static readonly DateTime SeedDate = new(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder modelBuilder)
    {
        var permissions = BuildPermissions();
        modelBuilder.Entity<Permission>().HasData(permissions);
        modelBuilder.Entity<Role>().HasData(BuildRoles());
        modelBuilder.Entity<RolePermission>().HasData(BuildRolePermissions(permissions));
        modelBuilder.Entity<UserProfile>().HasData(BuildAdminUser());
        modelBuilder.Entity<UserRole>().HasData(BuildAdminUserRole());
    }

    // ── Permissions ───────────────────────────────────────────────────────────

    private static List<Permission> BuildPermissions()
    {
        // Each entry: (Module, Action, DescriptionAr, DescriptionEn)
        var definitions = new (string Module, string Action, string Ar, string En)[]
        {
            // USERS
            ("USERS", "READ",   "عرض المستخدمين",          "View Users"),
            ("USERS", "CREATE", "إنشاء مستخدم",             "Create User"),
            ("USERS", "UPDATE", "تعديل مستخدم",             "Update User"),
            ("USERS", "DELETE", "حذف مستخدم",               "Delete User"),
            ("USERS", "MANAGE", "إدارة كاملة للمستخدمين",  "Full User Management"),
            // DEPARTMENTS
            ("DEPARTMENTS", "READ",   "عرض الأقسام",   "View Departments"),
            ("DEPARTMENTS", "CREATE", "إنشاء قسم",      "Create Department"),
            ("DEPARTMENTS", "UPDATE", "تعديل قسم",      "Update Department"),
            ("DEPARTMENTS", "DELETE", "حذف قسم",        "Delete Department"),
            // RACI
            ("RACI", "READ",    "عرض مصفوفة RACI",     "View RACI Matrix"),
            ("RACI", "CREATE",  "إنشاء RACI",           "Create RACI"),
            ("RACI", "UPDATE",  "تعديل RACI",           "Update RACI"),
            ("RACI", "DELETE",  "حذف RACI",             "Delete RACI"),
            ("RACI", "APPROVE", "اعتماد RACI",          "Approve RACI"),
            // POLICIES
            ("POLICIES", "READ",    "عرض السياسات",    "View Policies"),
            ("POLICIES", "CREATE",  "إنشاء سياسة",     "Create Policy"),
            ("POLICIES", "UPDATE",  "تعديل سياسة",     "Update Policy"),
            ("POLICIES", "DELETE",  "حذف سياسة",       "Delete Policy"),
            ("POLICIES", "APPROVE", "اعتماد سياسة",    "Approve Policy"),
            ("POLICIES", "PUBLISH", "نشر سياسة",       "Publish Policy"),
            // TASKS
            ("TASKS", "READ",   "عرض المهام",          "View Tasks"),
            ("TASKS", "CREATE", "إنشاء مهمة",          "Create Task"),
            ("TASKS", "UPDATE", "تعديل مهمة",          "Update Task"),
            ("TASKS", "DELETE", "حذف مهمة",            "Delete Task"),
            ("TASKS", "ASSIGN", "إسناد مهمة",          "Assign Task"),
            // KPI
            ("KPI", "READ",   "عرض مؤشرات الأداء",    "View KPIs"),
            ("KPI", "CREATE", "إنشاء مؤشر أداء",       "Create KPI"),
            ("KPI", "UPDATE", "تعديل مؤشر أداء",       "Update KPI"),
            ("KPI", "DELETE", "حذف مؤشر أداء",         "Delete KPI"),
            // RISKS
            ("RISKS", "READ",   "عرض المخاطر",         "View Risks"),
            ("RISKS", "CREATE", "إنشاء مخاطرة",        "Create Risk"),
            ("RISKS", "UPDATE", "تعديل مخاطرة",        "Update Risk"),
            ("RISKS", "DELETE", "حذف مخاطرة",          "Delete Risk"),
            // REPORTS
            ("REPORTS", "READ",   "عرض التقارير",      "View Reports"),
            ("REPORTS", "EXPORT", "تصدير التقارير",    "Export Reports"),
            // SETTINGS
            ("SETTINGS", "READ",   "عرض الإعدادات",   "View Settings"),
            ("SETTINGS", "UPDATE", "تعديل الإعدادات",  "Update Settings"),
            // AUDIT
            ("AUDIT", "READ",   "عرض سجل المراجعة",   "View Audit Log"),
            // CONTROLS
            ("CONTROLS", "READ",   "عرض اختبارات الضوابط",  "View Control Tests"),
            ("CONTROLS", "CREATE", "إنشاء اختبار ضابط",      "Create Control Test"),
            ("CONTROLS", "UPDATE", "تعديل اختبار ضابط",      "Update Control Test"),
            ("CONTROLS", "DELETE", "حذف اختبار ضابط",        "Delete Control Test"),
            // WORKFLOWS
            ("WORKFLOWS", "READ",   "عرض تعريفات سير العمل",   "View Workflow Definitions"),
            ("WORKFLOWS", "MANAGE", "إدارة سير العمل",          "Manage Workflow Definitions"),
            ("WORKFLOWS", "APPROVE","اعتماد في سير العمل",      "Approve Workflow Instances"),
        };

        return definitions.Select((d, i) => new Permission
        {
            Id            = i + 1,
            Module        = d.Module,
            Action        = d.Action,
            Code          = $"{d.Module}.{d.Action}",
            DescriptionAr = d.Ar,
            DescriptionEn = d.En,
            CreatedAt     = SeedDate,
            CreatedBy     = "System"
        }).ToList();
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    private static Role[] BuildRoles() =>
    [
        new() { Id = 1, Code = "ADMIN",   NameAr = "مدير النظام", NameEn = "System Admin",  IsSystemRole = true, IsActive = true, CreatedAt = SeedDate, CreatedBy = "System" },
        new() { Id = 2, Code = "MANAGER", NameAr = "مدير",        NameEn = "Manager",       IsSystemRole = true, IsActive = true, CreatedAt = SeedDate, CreatedBy = "System" },
        new() { Id = 3, Code = "USER",    NameAr = "مستخدم",      NameEn = "User",          IsSystemRole = true, IsActive = true, CreatedAt = SeedDate, CreatedBy = "System" },
        new() { Id = 4, Code = "VIEWER",  NameAr = "مشاهد",       NameEn = "Viewer",        IsSystemRole = true, IsActive = true, CreatedAt = SeedDate, CreatedBy = "System" },
    ];

    // ── Role Permissions ──────────────────────────────────────────────────────

    private static List<RolePermission> BuildRolePermissions(List<Permission> permissions)
    {
        var result = new List<RolePermission>();

        var byCode = permissions.ToDictionary(p => p.Code, p => p.Id);

        // ADMIN → all permissions
        var adminCodes = permissions.Select(p => p.Code);

        // MANAGER → all except USERS.MANAGE and SETTINGS.UPDATE
        var managerExclude = new HashSet<string> { "USERS.MANAGE", "SETTINGS.UPDATE" };
        var managerCodes = permissions.Select(p => p.Code).Where(c => !managerExclude.Contains(c));

        // USER → all READ + TASKS.CREATE + TASKS.UPDATE + TASKS.ASSIGN
        var userCodes = permissions
            .Where(p => p.Action == "READ"
                     || p.Code is "TASKS.CREATE" or "TASKS.UPDATE" or "TASKS.ASSIGN")
            .Select(p => p.Code);

        // VIEWER → all READ only
        var viewerCodes = permissions.Where(p => p.Action == "READ").Select(p => p.Code);

        AddRolePermissions(result, roleId: 1, adminCodes,   byCode);
        AddRolePermissions(result, roleId: 2, managerCodes, byCode);
        AddRolePermissions(result, roleId: 3, userCodes,    byCode);
        AddRolePermissions(result, roleId: 4, viewerCodes,  byCode);

        return result;
    }

    private static void AddRolePermissions(
        List<RolePermission> list, int roleId,
        IEnumerable<string> codes, Dictionary<string, int> byCode)
    {
        foreach (var code in codes.Distinct())
        {
            if (!byCode.TryGetValue(code, out var permId)) continue;
            // Avoid duplicates (USER and VIEWER share READ permissions)
            if (list.Any(rp => rp.RoleId == roleId && rp.PermissionId == permId)) continue;
            list.Add(new RolePermission
            {
                RoleId       = roleId,
                PermissionId = permId,
                GrantedAt    = SeedDate,
                GrantedBy    = "System"
            });
        }
    }

    // ── Admin User ────────────────────────────────────────────────────────────

    private static UserProfile BuildAdminUser() => new()
    {
        Id           = 1,
        Username     = "admin",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        FullNameAr   = "مدير النظام",
        FullNameEn   = "System Administrator",
        Email        = "admin@igms.local",
        IsActive     = true,
        CreatedAt    = SeedDate,
        CreatedBy    = "System"
    };

    private static UserRole BuildAdminUserRole() => new()
    {
        UserId     = 1,
        RoleId     = 1,
        AssignedBy = "System",
        AssignedAt = SeedDate
    };
}
