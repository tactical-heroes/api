using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core.Migrations
{
    public partial class RenameFolderAllowedFileType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "type",
                schema: "file_manager",
                table: "folders",
                newName: "allowed_file_type");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "allowed_file_type",
                schema: "file_manager",
                table: "folders",
                newName: "type");
        }
    }
}
