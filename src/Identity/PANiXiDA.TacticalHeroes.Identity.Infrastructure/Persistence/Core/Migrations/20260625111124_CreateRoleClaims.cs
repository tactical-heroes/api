using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260625111124_CreateRoleClaims")]
public partial class CreateRoleClaims : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "identity_role_claims",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                identity_role_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_identity_role_claims", x => x.id);
                table.ForeignKey(
                    name: "fk_identity_role_claims_identity_roles_identity_role_id",
                    column: x => x.identity_role_id,
                    principalTable: "identity_roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_identity_role_claims_identity_role_id_type_value",
            table: "identity_role_claims",
            columns: new[] { "identity_role_id", "type", "value" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "identity_role_claims");
    }
}
