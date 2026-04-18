using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <summary>
    /// DEV NOTE: Was fixing test user password hashes. Moved to DevDataSeeder.
    /// Intentionally empty – does not run against production tenant databases.
    /// </summary>
    public partial class FixTestUserPasswords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) { }
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
