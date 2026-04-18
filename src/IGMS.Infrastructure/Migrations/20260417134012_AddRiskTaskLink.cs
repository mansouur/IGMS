using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskTaskLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RiskId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$I9ZDNJda8DT8QwBAxDIBGOsCg10c23BDKMhnw9hdvJcu0cqTIByhC");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_RiskId",
                table: "Tasks",
                column: "RiskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Risks_RiskId",
                table: "Tasks",
                column: "RiskId",
                principalTable: "Risks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Risks_RiskId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_RiskId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RiskId",
                table: "Tasks");

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$k6kRKE9U0r/PZ4J9gKFP0uBZnOECyAMskM4oaxGeoqQXGmAmQR1g6");
        }
    }
}
