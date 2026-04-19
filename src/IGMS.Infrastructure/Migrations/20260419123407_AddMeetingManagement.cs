using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetingManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AgendaAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinutesAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotesAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OrganizerId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meetings_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Meetings_UserProfiles_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingActionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingId = table.Column<int>(type: "int", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AssignedToId = table.Column<int>(type: "int", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_MeetingActionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingActionItems_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingActionItems_UserProfiles_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingAttendees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    RoleInMeeting = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_MeetingAttendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingAttendees_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingAttendees_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$PDxIpqHN.azx8qmKdp912O0Z3iCyjT7MBMuZzY13KMeCeAVCEKNeO");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingActionItems_AssignedToId",
                table: "MeetingActionItems",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingActionItems_MeetingId",
                table: "MeetingActionItems",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingAttendees_MeetingId",
                table: "MeetingAttendees",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingAttendees_UserId",
                table: "MeetingAttendees",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_DepartmentId",
                table: "Meetings",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_OrganizerId",
                table: "Meetings",
                column: "OrganizerId");

            // ── MEETINGS permissions ──────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: ["Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy",
                          "DescriptionAr", "DescriptionEn", "IsDeleted", "ModifiedAt", "ModifiedBy", "Module"],
                values: new object[,]
                {
                    { "READ",     "MEETINGS.READ",     new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "عرض الاجتماعات",   "View Meetings",   false, null, null, "MEETINGS" },
                    { "CREATE",   "MEETINGS.CREATE",   new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إضافة الاجتماعات", "Create Meetings", false, null, null, "MEETINGS" },
                    { "UPDATE",   "MEETINGS.UPDATE",   new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "تعديل الاجتماعات", "Update Meetings", false, null, null, "MEETINGS" },
                    { "DELETE",   "MEETINGS.DELETE",   new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "حذف الاجتماعات",   "Delete Meetings", false, null, null, "MEETINGS" },
                    { "MANAGE",   "MEETINGS.MANAGE",   new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "إدارة الاجتماعات", "Manage Meetings", false, null, null, "MEETINGS" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingActionItems");

            migrationBuilder.DropTable(
                name: "MeetingAttendees");

            migrationBuilder.DropTable(
                name: "Meetings");

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$74LhuhHt5sYT6l5P4GrQqeYkUqP5XF3qpqBG9Cx37gK5W4p5ThjZe");
        }
    }
}
