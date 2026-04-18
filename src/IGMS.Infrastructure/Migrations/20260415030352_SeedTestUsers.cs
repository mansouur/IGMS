using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IGMS.Infrastructure.Migrations
{
    /// <summary>
    /// DEV NOTE: Test user data was originally here. Moved to DevDataSeeder.
    /// This migration is intentionally empty so production tenants are not affected.
    /// To seed a dev DB run: dotnet run --project src/IGMS.API -- seed-dev
    /// </summary>
    public partial class SeedTestUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) { }
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
