using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core.Migrations
{
    public partial class AddPersonalFileOwnership : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                schema: "file_manager",
                table: "folders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                schema: "file_manager",
                table: "files",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_folders_user_id",
                schema: "file_manager",
                table: "folders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_files_user_id",
                schema: "file_manager",
                table: "files",
                column: "user_id");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_folders_user_id",
                schema: "file_manager",
                table: "folders");

            migrationBuilder.DropIndex(
                name: "ix_files_user_id",
                schema: "file_manager",
                table: "files");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "file_manager",
                table: "folders");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "file_manager",
                table: "files");
        }
    }
}
