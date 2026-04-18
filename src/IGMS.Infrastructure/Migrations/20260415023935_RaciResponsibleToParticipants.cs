using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RaciResponsibleToParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaciActivities_UserProfiles_ResponsibleUserId",
                table: "RaciActivities");

            migrationBuilder.DropIndex(
                name: "IX_RaciActivities_ResponsibleUserId",
                table: "RaciActivities");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserId",
                table: "RaciActivities");

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$2n0w77Xg628CvFW6dCghsOoXvxIEmS0WPuzFOsbkbEEfEUP5jfinu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResponsibleUserId",
                table: "RaciActivities",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$NDmtmzaKMrOxXtimmSOWOuflCu1SpqLvvo4fKZALftDYSA.5jhO.O");

            migrationBuilder.CreateIndex(
                name: "IX_RaciActivities_ResponsibleUserId",
                table: "RaciActivities",
                column: "ResponsibleUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RaciActivities_UserProfiles_ResponsibleUserId",
                table: "RaciActivities",
                column: "ResponsibleUserId",
                principalTable: "UserProfiles",
                principalColumn: "Id");
        }
    }
}
