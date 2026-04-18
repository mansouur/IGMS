using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddControlsPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DescriptionAr", "DescriptionEn", "IsDeleted", "ModifiedAt", "ModifiedBy", "Module" },
                values: new object[,]
                {
                    { 39, "READ", "CONTROLS.READ", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض اختبارات الضوابط", "View Control Tests", false, null, null, "CONTROLS" },
                    { 40, "CREATE", "CONTROLS.CREATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إنشاء اختبار ضابط", "Create Control Test", false, null, null, "CONTROLS" },
                    { 41, "UPDATE", "CONTROLS.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل اختبار ضابط", "Update Control Test", false, null, null, "CONTROLS" },
                    { 42, "DELETE", "CONTROLS.DELETE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف اختبار ضابط", "Delete Control Test", false, null, null, "CONTROLS" }
                });

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$hl2SuX2lk/UCw9yeIca.4OX6Hsmls801uY2uMAb86dBnRcOv4Hp.S");

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAt", "GrantedBy" },
                values: new object[,]
                {
                    { 39, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 40, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 41, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 42, 1, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 39, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 40, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 41, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 42, 2, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 39, 3, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 39, 4, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 39, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 40, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 41, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 42, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 39, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 40, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 41, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 42, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 39, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 39, 4 });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$2O6pkjVJs5llEcx2cZwIW.gTDrodyDpFjhNK45gZ9JYW6v0EJJ7BC");
        }
    }
}
