using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Core.Migrations
{
    public partial class RenameFileOperationAttemptId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "content_update_attempt_id",
                schema: "file_manager",
                table: "files",
                newName: "operation_attempt_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "operation_attempt_id",
                schema: "file_manager",
                table: "files",
                newName: "content_update_attempt_id");
        }
    }
}
