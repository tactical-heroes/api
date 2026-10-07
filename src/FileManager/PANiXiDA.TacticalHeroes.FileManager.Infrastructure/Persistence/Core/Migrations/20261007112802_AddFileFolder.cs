using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core.Migrations
{
    public partial class AddFileFolder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "folder_id",
                schema: "file_manager",
                table: "files",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_files_folder_id",
                schema: "file_manager",
                table: "files",
                column: "folder_id");

            migrationBuilder.AddForeignKey(
                name: "fk_files_folder_folder_id",
                schema: "file_manager",
                table: "files",
                column: "folder_id",
                principalSchema: "file_manager",
                principalTable: "folders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_files_folder_folder_id",
                schema: "file_manager",
                table: "files");

            migrationBuilder.DropIndex(
                name: "ix_files_folder_id",
                schema: "file_manager",
                table: "files");

            migrationBuilder.DropColumn(
                name: "folder_id",
                schema: "file_manager",
                table: "files");
        }
    }
}
