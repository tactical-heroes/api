using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260625111122_CreateOpenIddictApplications")]
public partial class CreateOpenIddictApplications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "open_iddict_applications",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                application_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                client_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                client_secret = table.Column<string>(type: "text", nullable: true),
                client_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                consent_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                display_name = table.Column<string>(type: "text", nullable: true),
                display_names = table.Column<string>(type: "text", nullable: true),
                json_web_key_set = table.Column<string>(type: "text", nullable: true),
                permissions = table.Column<string>(type: "text", nullable: true),
                post_logout_redirect_uris = table.Column<string>(type: "text", nullable: true),
                properties = table.Column<string>(type: "text", nullable: true),
                redirect_uris = table.Column<string>(type: "text", nullable: true),
                requirements = table.Column<string>(type: "text", nullable: true),
                settings = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_open_iddict_applications", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_open_iddict_applications_client_id",
            schema: "identity",
            table: "open_iddict_applications",
            column: "client_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "open_iddict_applications",
            schema: "identity");
    }
}
