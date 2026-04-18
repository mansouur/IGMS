using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FullNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProfileImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdObjectId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UaePassSub = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmiratesId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DescriptionAr", "DescriptionEn", "IsDeleted", "ModifiedAt", "ModifiedBy", "Module" },
                values: new object[,]
                {
                    { 1, "READ", "USERS.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض المستخدمين", "View Users", false, null, null, "USERS" },
                    { 2, "CREATE", "USERS.CREATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إنشاء مستخدم", "Create User", false, null, null, "USERS" },
                    { 3, "UPDATE", "USERS.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل مستخدم", "Update User", false, null, null, "USERS" },
                    { 4, "DELETE", "USERS.DELETE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف مستخدم", "Delete User", false, null, null, "USERS" },
                    { 5, "MANAGE", "USERS.MANAGE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إدارة كاملة للمستخدمين", "Full User Management", false, null, null, "USERS" },
                    { 6, "READ", "DEPARTMENTS.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض الأقسام", "View Departments", false, null, null, "DEPARTMENTS" },
                    { 7, "CREATE", "DEPARTMENTS.CREATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إنشاء قسم", "Create Department", false, null, null, "DEPARTMENTS" },
                    { 8, "UPDATE", "DEPARTMENTS.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل قسم", "Update Department", false, null, null, "DEPARTMENTS" },
                    { 9, "DELETE", "DEPARTMENTS.DELETE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف قسم", "Delete Department", false, null, null, "DEPARTMENTS" },
                    { 10, "READ", "RACI.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض مصفوفة RACI", "View RACI Matrix", false, null, null, "RACI" },
                    { 11, "CREATE", "RACI.CREATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إنشاء RACI", "Create RACI", false, null, null, "RACI" },
                    { 12, "UPDATE", "RACI.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل RACI", "Update RACI", false, null, null, "RACI" },
                    { 13, "DELETE", "RACI.DELETE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف RACI", "Delete RACI", false, null, null, "RACI" },
                    { 14, "APPROVE", "RACI.APPROVE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "اعتماد RACI", "Approve RACI", false, null, null, "RACI" },
                    { 15, "READ", "POLICIES.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض السياسات", "View Policies", false, null, null, "POLICIES" },
                    { 16, "CREATE", "POLICIES.CREATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إنشاء سياسة", "Create Policy", false, null, null, "POLICIES" },
                    { 17, "UPDATE", "POLICIES.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل سياسة", "Update Policy", false, null, null, "POLICIES" },
                    { 18, "DELETE", "POLICIES.DELETE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف سياسة", "Delete Policy", false, null, null, "POLICIES" },
                    { 19, "APPROVE", "POLICIES.APPROVE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "اعتماد سياسة", "Approve Policy", false, null, null, "POLICIES" },
                    { 20, "PUBLISH", "POLICIES.PUBLISH", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "نشر سياسة", "Publish Policy", false, null, null, "POLICIES" },
                    { 21, "READ", "TASKS.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض المهام", "View Tasks", false, null, null, "TASKS" },
                    { 22, "CREATE", "TASKS.CREATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إنشاء مهمة", "Create Task", false, null, null, "TASKS" },
                    { 23, "UPDATE", "TASKS.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل مهمة", "Update Task", false, null, null, "TASKS" },
                    { 24, "DELETE", "TASKS.DELETE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف مهمة", "Delete Task", false, null, null, "TASKS" },
                    { 25, "ASSIGN", "TASKS.ASSIGN", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إسناد مهمة", "Assign Task", false, null, null, "TASKS" },
                    { 26, "READ", "KPI.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض مؤشرات الأداء", "View KPIs", false, null, null, "KPI" },
                    { 27, "CREATE", "KPI.CREATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إنشاء مؤشر أداء", "Create KPI", false, null, null, "KPI" },
                    { 28, "UPDATE", "KPI.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل مؤشر أداء", "Update KPI", false, null, null, "KPI" },
                    { 29, "DELETE", "KPI.DELETE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف مؤشر أداء", "Delete KPI", false, null, null, "KPI" },
                    { 30, "READ", "RISKS.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض المخاطر", "View Risks", false, null, null, "RISKS" },
                    { 31, "CREATE", "RISKS.CREATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إنشاء مخاطرة", "Create Risk", false, null, null, "RISKS" },
                    { 32, "UPDATE", "RISKS.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل مخاطرة", "Update Risk", false, null, null, "RISKS" },
                    { 33, "DELETE", "RISKS.DELETE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف مخاطرة", "Delete Risk", false, null, null, "RISKS" },
                    { 34, "READ", "REPORTS.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض التقارير", "View Reports", false, null, null, "REPORTS" },
                    { 35, "EXPORT", "REPORTS.EXPORT", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تصدير التقارير", "Export Reports", false, null, null, "REPORTS" },
                    { 36, "READ", "SETTINGS.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض الإعدادات", "View Settings", false, null, null, "SETTINGS" },
                    { 37, "UPDATE", "SETTINGS.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل الإعدادات", "Update Settings", false, null, null, "SETTINGS" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DescriptionAr", "DescriptionEn", "IsActive", "IsDeleted", "IsSystemRole", "ModifiedAt", "ModifiedBy", "NameAr", "NameEn" },
                values: new object[,]
                {
                    { 1, "ADMIN", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, true, false, true, null, null, "مدير النظام", "System Admin" },
                    { 2, "MANAGER", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, true, false, true, null, null, "مدير", "Manager" },
                    { 3, "USER", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, true, false, true, null, null, "مستخدم", "User" },
                    { 4, "VIEWER", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, null, true, false, true, null, null, "مشاهد", "Viewer" }
                });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "Id", "AdObjectId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Email", "EmiratesId", "FullNameAr", "FullNameEn", "IsActive", "IsDeleted", "LastLoginAt", "ModifiedAt", "ModifiedBy", "PasswordHash", "PhoneNumber", "ProfileImagePath", "UaePassSub", "Username" },
                values: new object[] { 1, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, null, "admin@igms.local", null, "مدير النظام", "System Administrator", true, false, null, null, null, "$2a$11$2HRksdQjxpgM7w4CV5y6ZeDqxs9mpGYZyqvlmaA4tdYHJ0wSc.yay", null, null, null, "admin" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAt", "GrantedBy" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 2, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 3, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 4, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 5, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 6, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 7, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 8, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 9, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 10, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 11, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 12, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 13, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 14, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 15, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 16, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 17, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 18, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 19, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 20, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 21, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 22, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 23, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 24, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 25, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 26, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 27, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 28, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 29, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 30, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 31, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 32, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 33, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 34, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 35, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 36, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 37, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 1, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 2, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 3, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 4, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 6, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 7, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 8, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 9, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 10, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 11, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 12, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 13, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 14, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 15, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 16, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 17, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 18, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 19, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 20, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 21, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 22, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 23, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 24, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 25, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 26, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 27, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 28, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 29, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 30, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 31, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 32, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 33, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 34, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 35, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 36, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 1, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 6, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 10, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 15, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 21, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 22, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 23, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 25, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 26, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 30, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 34, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 36, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 1, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 6, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 10, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 15, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 21, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 26, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 30, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 34, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 36, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "AssignedAt", "AssignedBy", "ExpiresAt" },
                values: new object[] { 1, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName",
                table: "AuditLogs",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IsDeleted",
                table: "Departments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentId",
                table: "Departments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_IsDeleted",
                table: "Permissions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Module",
                table: "Permissions",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                table: "Roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_IsDeleted",
                table: "Roles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_AdObjectId",
                table: "UserProfiles",
                column: "AdObjectId",
                unique: true,
                filter: "[AdObjectId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_DepartmentId",
                table: "UserProfiles",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Email",
                table: "UserProfiles",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_EmiratesId",
                table: "UserProfiles",
                column: "EmiratesId",
                unique: true,
                filter: "[EmiratesId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_IsActive",
                table: "UserProfiles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_IsDeleted",
                table: "UserProfiles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UaePassSub",
                table: "UserProfiles",
                column: "UaePassSub",
                unique: true,
                filter: "[UaePassSub] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Username",
                table: "UserProfiles",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_ExpiresAt",
                table: "UserRoles",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_UserProfiles_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_UserProfiles_ManagerId",
                table: "Departments",
                column: "ManagerId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_UserProfiles_ManagerId",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
