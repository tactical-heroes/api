using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20261018100000_AddDefaultRole")]
public partial class AddDefaultRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_default",
            schema: "identity",
            table: "asp_net_roles",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "ix_asp_net_roles_is_default",
            schema: "identity",
            table: "asp_net_roles",
            column: "is_default",
            unique: true,
            filter: "is_default");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_asp_net_roles_is_default",
            schema: "identity",
            table: "asp_net_roles");

        migrationBuilder.DropColumn(
            name: "is_default",
            schema: "identity",
            table: "asp_net_roles");
    }
}
