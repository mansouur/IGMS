using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRaciTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RaciMatrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_RaciMatrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaciMatrices_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RaciMatrices_UserProfiles_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RaciActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    RaciMatrixId = table.Column<int>(type: "int", nullable: false),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true),
                    AccountableUserId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_RaciActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaciActivities_RaciMatrices_RaciMatrixId",
                        column: x => x.RaciMatrixId,
                        principalTable: "RaciMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaciActivities_UserProfiles_AccountableUserId",
                        column: x => x.AccountableUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RaciActivities_UserProfiles_ResponsibleUserId",
                        column: x => x.ResponsibleUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RaciParticipants",
                columns: table => new
                {
                    RaciActivityId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaciParticipants", x => new { x.RaciActivityId, x.UserId, x.Role });
                    table.ForeignKey(
                        name: "FK_RaciParticipants_RaciActivities_RaciActivityId",
                        column: x => x.RaciActivityId,
                        principalTable: "RaciActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaciParticipants_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$NDmtmzaKMrOxXtimmSOWOuflCu1SpqLvvo4fKZALftDYSA.5jhO.O");

            migrationBuilder.CreateIndex(
                name: "IX_RaciActivities_AccountableUserId",
                table: "RaciActivities",
                column: "AccountableUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RaciActivities_DisplayOrder",
                table: "RaciActivities",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_RaciActivities_RaciMatrixId",
                table: "RaciActivities",
                column: "RaciMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_RaciActivities_ResponsibleUserId",
                table: "RaciActivities",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RaciMatrices_ApprovedById",
                table: "RaciMatrices",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_RaciMatrices_DepartmentId",
                table: "RaciMatrices",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RaciMatrices_IsDeleted",
                table: "RaciMatrices",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_RaciMatrices_Status",
                table: "RaciMatrices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RaciParticipants_RaciActivityId_Role",
                table: "RaciParticipants",
                columns: new[] { "RaciActivityId", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_RaciParticipants_UserId",
                table: "RaciParticipants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaciParticipants");

            migrationBuilder.DropTable(
                name: "RaciActivities");

            migrationBuilder.DropTable(
                name: "RaciMatrices");

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$WLys0ifNnf6UdpAopNEoyOddRwWcswqPhgXwhWaLVZsPg5JcNA4i2");
        }
    }
}
