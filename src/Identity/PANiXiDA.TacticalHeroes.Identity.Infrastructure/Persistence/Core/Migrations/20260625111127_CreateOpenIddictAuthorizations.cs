using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260625111127_CreateOpenIddictAuthorizations")]
public partial class CreateOpenIddictAuthorizations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "open_iddict_authorizations",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                application_id = table.Column<Guid>(type: "uuid", nullable: true),
                concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                properties = table.Column<string>(type: "text", nullable: true),
                scopes = table.Column<string>(type: "text", nullable: true),
                status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_open_iddict_authorizations", x => x.id);
                table.ForeignKey(
                    name: "fk_open_iddict_authorizations_open_iddict_applications_applica",
                    column: x => x.application_id,
                    principalSchema: "identity",
                    principalTable: "open_iddict_applications",
                    principalColumn: "id");
            });

        migrationBuilder.CreateIndex(
            name: "ix_open_iddict_authorizations_application_id_status_subject_ty",
            schema: "identity",
            table: "open_iddict_authorizations",
            columns: new[] { "application_id", "status", "subject", "type" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "open_iddict_authorizations",
            schema: "identity");
    }
}
