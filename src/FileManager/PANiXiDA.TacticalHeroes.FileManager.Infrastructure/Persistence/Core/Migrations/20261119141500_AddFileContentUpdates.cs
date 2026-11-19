using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core.Migrations
{
    public partial class AddFileContentUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "version_id",
                schema: "file_manager",
                table: "files",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "content_update_attempt_id",
                schema: "file_manager",
                table: "files",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version_id",
                schema: "file_manager",
                table: "files");
            migrationBuilder.DropColumn(
                name: "content_update_attempt_id",
                schema: "file_manager",
                table: "files");
        }
    }
}
