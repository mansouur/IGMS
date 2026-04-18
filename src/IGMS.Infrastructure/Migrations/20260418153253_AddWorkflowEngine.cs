using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$GWPNNuiSHVA.mrRFvbp0xO9HP1yz2nnsRLMAhtGZQ82c7GoJeccva");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$hl2SuX2lk/UCw9yeIca.4OX6Hsmls801uY2uMAb86dBnRcOv4Hp.S");
        }
    }
}
