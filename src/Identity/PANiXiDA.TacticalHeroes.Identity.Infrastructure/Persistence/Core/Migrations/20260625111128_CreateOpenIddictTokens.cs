using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260625111128_CreateOpenIddictTokens")]
public partial class CreateOpenIddictTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OpenIddictTokens",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                application_id = table.Column<Guid>(type: "uuid", nullable: true),
                authorization_id = table.Column<Guid>(type: "uuid", nullable: true),
                concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                payload = table.Column<string>(type: "text", nullable: true),
                properties = table.Column<string>(type: "text", nullable: true),
                redemption_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_open_iddict_tokens", x => x.id);
                table.ForeignKey(
                    name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                    column: x => x.application_id,
                    principalSchema: "identity",
                    principalTable: "OpenIddictApplications",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                    column: x => x.authorization_id,
                    principalSchema: "identity",
                    principalTable: "OpenIddictAuthorizations",
                    principalColumn: "id");
            });

        migrationBuilder.CreateIndex(
            name: "ix_open_iddict_tokens_application_id_status_subject_type",
            schema: "identity",
            table: "OpenIddictTokens",
            columns: new[] { "application_id", "status", "subject", "type" });

        migrationBuilder.CreateIndex(
            name: "ix_open_iddict_tokens_authorization_id",
            schema: "identity",
            table: "OpenIddictTokens",
            column: "authorization_id");

        migrationBuilder.CreateIndex(
            name: "ix_open_iddict_tokens_reference_id",
            schema: "identity",
            table: "OpenIddictTokens",
            column: "reference_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OpenIddictTokens",
            schema: "identity");
    }
}
