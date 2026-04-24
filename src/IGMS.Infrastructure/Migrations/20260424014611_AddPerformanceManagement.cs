using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerformanceReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OverallRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    StrengthsAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreasForImprovementAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommentsAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeCommentsAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_UserProfiles_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_UserProfiles_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ActualValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceGoals_PerformanceReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "PerformanceReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$O/NFTT2gupbZwGn93RD5EeMMY68LBnT5BczuL4Q6D3KV7JLz0qYci");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceGoals_ReviewId",
                table: "PerformanceGoals",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_DepartmentId",
                table: "PerformanceReviews",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_EmployeeId",
                table: "PerformanceReviews",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_ReviewerId",
                table: "PerformanceReviews",
                column: "ReviewerId");

            // ── PERFORMANCE permissions ───────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: ["Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy",
                          "DescriptionAr", "DescriptionEn", "IsDeleted", "ModifiedAt", "ModifiedBy", "Module"],
                values: new object[,]
                {
                    { "READ",   "PERFORMANCE.READ",   new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض تقييمات الأداء",   "View Performance Reviews",   false, null, null, "PERFORMANCE" },
                    { "CREATE", "PERFORMANCE.CREATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إنشاء تقييمات الأداء", "Create Performance Reviews", false, null, null, "PERFORMANCE" },
                    { "UPDATE", "PERFORMANCE.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل تقييمات الأداء", "Update Performance Reviews", false, null, null, "PERFORMANCE" },
                    { "DELETE", "PERFORMANCE.DELETE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف تقييمات الأداء",   "Delete Performance Reviews", false, null, null, "PERFORMANCE" },
                    { "APPROVE","PERFORMANCE.APPROVE",new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "اعتماد تقييمات الأداء","Approve Performance Reviews",false, null, null, "PERFORMANCE" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerformanceGoals");

            migrationBuilder.DropTable(
                name: "PerformanceReviews");

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$PDxIpqHN.azx8qmKdp912O0Z3iCyjT7MBMuZzY13KMeCeAVCEKNeO");
        }
    }
}
