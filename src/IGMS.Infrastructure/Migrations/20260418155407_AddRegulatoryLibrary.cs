using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegulatoryLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$DrYFoU8C6Uqj52lv6e2LGOrHE3L9Ov.he.l31YSsh3gRuGMYx4bRq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$E1Z716CQAXeUigYowprYRuvV6EP9C3MEpqaw3t9NiCaIz8rgDF.ky");
        }
    }
}
