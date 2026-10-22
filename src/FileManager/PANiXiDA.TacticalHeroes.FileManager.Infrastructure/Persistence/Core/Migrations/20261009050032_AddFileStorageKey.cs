using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core.Migrations
{
    public partial class AddFileStorageKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "storage_key",
                schema: "file_manager",
                table: "files",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "ix_files_storage_key",
                schema: "file_manager",
                table: "files",
                column: "storage_key",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_files_storage_key",
                schema: "file_manager",
                table: "files");

            migrationBuilder.DropColumn(
                name: "storage_key",
                schema: "file_manager",
                table: "files");
        }
    }
}
