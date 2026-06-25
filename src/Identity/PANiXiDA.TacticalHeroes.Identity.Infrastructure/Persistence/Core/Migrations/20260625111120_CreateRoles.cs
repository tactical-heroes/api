using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260625111120_CreateRoles")]
public partial class CreateRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "identity_roles",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_identity_roles", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_identity_roles_name",
            table: "identity_roles",
            column: "name",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "identity_roles");
    }
}
