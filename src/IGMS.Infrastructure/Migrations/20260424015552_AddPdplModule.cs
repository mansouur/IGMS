using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPdplModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PdplRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleAr = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PurposeAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataSubjectsAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetentionPeriod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SecurityMeasures = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCategory = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LegalBasis = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsThirdPartySharing = table.Column<bool>(type: "bit", nullable: false),
                    ThirdPartyDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCrossBorderTransfer = table.Column<bool>(type: "bit", nullable: false),
                    TransferCountry = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TransferSafeguards = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_PdplRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PdplRecords_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PdplRecords_UserProfiles_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PdplConsents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PdplRecordId = table.Column<int>(type: "int", nullable: false),
                    SubjectNameAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SubjectEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubjectIdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsConsented = table.Column<bool>(type: "bit", nullable: false),
                    ConsentedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WithdrawnAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PdplConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PdplConsents_PdplRecords_PdplRecordId",
                        column: x => x.PdplRecordId,
                        principalTable: "PdplRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PdplDataRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PdplRecordId = table.Column<int>(type: "int", nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubjectNameAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SubjectEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DetailsAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedToId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_PdplDataRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PdplDataRequests_PdplRecords_PdplRecordId",
                        column: x => x.PdplRecordId,
                        principalTable: "PdplRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PdplDataRequests_UserProfiles_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$g1IcNGCEofLG0aCMDOKlp.2D.krBDyp9w0YrEvUgpLTdkiY8qNZj2");

            migrationBuilder.CreateIndex(
                name: "IX_PdplConsents_PdplRecordId",
                table: "PdplConsents",
                column: "PdplRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PdplDataRequests_AssignedToId",
                table: "PdplDataRequests",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_PdplDataRequests_PdplRecordId",
                table: "PdplDataRequests",
                column: "PdplRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PdplRecords_DepartmentId",
                table: "PdplRecords",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PdplRecords_OwnerId",
                table: "PdplRecords",
                column: "OwnerId");

            // ── PDPL permissions ──────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: ["Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy",
                          "DescriptionAr", "DescriptionEn", "IsDeleted", "ModifiedAt", "ModifiedBy", "Module"],
                values: new object[,]
                {
                    { "READ",   "PDPL.READ",   new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض سجلات حماية البيانات",    "View PDPL Records",    false, null, null, "PDPL" },
                    { "CREATE", "PDPL.CREATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إنشاء سجلات حماية البيانات",  "Create PDPL Records",  false, null, null, "PDPL" },
                    { "UPDATE", "PDPL.UPDATE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل سجلات حماية البيانات",  "Update PDPL Records",  false, null, null, "PDPL" },
                    { "DELETE", "PDPL.DELETE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف سجلات حماية البيانات",    "Delete PDPL Records",  false, null, null, "PDPL" },
                    { "MANAGE", "PDPL.MANAGE", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إدارة الطلبات والموافقات PDPL","Manage PDPL Requests", false, null, null, "PDPL" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PdplConsents");

            migrationBuilder.DropTable(
                name: "PdplDataRequests");

            migrationBuilder.DropTable(
                name: "PdplRecords");

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$O/NFTT2gupbZwGn93RD5EeMMY68LBnT5BczuL4Q6D3KV7JLz0qYci");
        }
    }
}
