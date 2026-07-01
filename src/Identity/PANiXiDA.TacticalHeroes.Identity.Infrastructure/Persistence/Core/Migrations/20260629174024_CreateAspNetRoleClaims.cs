using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260629174024_CreateAspNetRoleClaims")]
public partial class CreateAspNetRoleClaims : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "asp_net_role_claims",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                claim_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                claim_value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                table.ForeignKey(
                    name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: "identity",
                    principalTable: "asp_net_roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_asp_net_role_claims_role_id_claim_type_claim_value",
            schema: "identity",
            table: "asp_net_role_claims",
            columns: new[] { "role_id", "claim_type", "claim_value" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "asp_net_role_claims",
            schema: "identity");
    }
}
