using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core.Migrations
{
    public partial class UseFileSoftDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE file_manager.files
                SET status = 'Deleting',
                    deleted_at = COALESCE(deleted_at, updated_at),
                    updated_at = CURRENT_TIMESTAMP
                WHERE status = 'Deleted';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Keep deleted rows hidden on rollback rather than restoring removed files.
        }
    }
}
