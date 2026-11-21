using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core.Migrations
{
    public partial class AddFileRelocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "relocation_attempt_id",
                schema: "file_manager",
                table: "files",
                type: "uuid",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "relocation_name",
                schema: "file_manager",
                table: "files",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "relocation_type",
                schema: "file_manager",
                table: "files",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "relocation_user_id",
                schema: "file_manager",
                table: "files",
                type: "uuid",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "relocation_folder_id",
                schema: "file_manager",
                table: "files",
                type: "uuid",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "relocation_storage_key",
                schema: "file_manager",
                table: "files",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "relocation_attempt_id",
                schema: "file_manager",
                table: "files");
            migrationBuilder.DropColumn(
                name: "relocation_name",
                schema: "file_manager",
                table: "files");
            migrationBuilder.DropColumn(
                name: "relocation_type",
                schema: "file_manager",
                table: "files");
            migrationBuilder.DropColumn(
                name: "relocation_user_id",
                schema: "file_manager",
                table: "files");
            migrationBuilder.DropColumn(
                name: "relocation_folder_id",
                schema: "file_manager",
                table: "files");
            migrationBuilder.DropColumn(
                name: "relocation_storage_key",
                schema: "file_manager",
                table: "files");
        }
    }
}
