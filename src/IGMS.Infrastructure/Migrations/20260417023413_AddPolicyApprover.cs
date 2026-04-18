using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyApprover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_RaciMatrices_RaciMatrixId",
                table: "Policies");

            migrationBuilder.RenameColumn(
                name: "RaciMatrixId",
                table: "Policies",
                newName: "ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_Policies_RaciMatrixId",
                table: "Policies",
                newName: "IX_Policies_ApproverId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Policies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$sCPjYN12KE3OtMTaPj7W3u2Dqh3BT6xv281M1NgjhzlGxEdd.7bHa");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_UserProfiles_ApproverId",
                table: "Policies",
                column: "ApproverId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_UserProfiles_ApproverId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Policies");

            migrationBuilder.RenameColumn(
                name: "ApproverId",
                table: "Policies",
                newName: "RaciMatrixId");

            migrationBuilder.RenameIndex(
                name: "IX_Policies_ApproverId",
                table: "Policies",
                newName: "IX_Policies_RaciMatrixId");

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$OXBdo.8FRRfukVSYVq6ScuwrIqGKbd3dtclsZfl9TteF/q8viby7G");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_RaciMatrices_RaciMatrixId",
                table: "Policies",
                column: "RaciMatrixId",
                principalTable: "RaciMatrices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
