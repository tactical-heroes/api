using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Core.Migrations
{
    public partial class AddFolderConcurrencyToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "file_manager",
                table: "folders",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "file_manager",
                table: "folders");
        }
    }
}
