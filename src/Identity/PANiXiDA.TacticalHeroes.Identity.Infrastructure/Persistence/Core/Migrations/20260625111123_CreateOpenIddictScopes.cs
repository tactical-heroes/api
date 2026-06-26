using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260625111123_CreateOpenIddictScopes")]
public partial class CreateOpenIddictScopes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "open_iddict_scopes",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                description = table.Column<string>(type: "text", nullable: true),
                descriptions = table.Column<string>(type: "text", nullable: true),
                display_name = table.Column<string>(type: "text", nullable: true),
                display_names = table.Column<string>(type: "text", nullable: true),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                properties = table.Column<string>(type: "text", nullable: true),
                resources = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_open_iddict_scopes", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_open_iddict_scopes_name",
            schema: "identity",
            table: "open_iddict_scopes",
            column: "name",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "open_iddict_scopes",
            schema: "identity");
    }
}
