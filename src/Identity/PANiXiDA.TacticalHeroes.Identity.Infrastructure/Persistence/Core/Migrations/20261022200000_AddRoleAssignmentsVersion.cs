using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20261022200000_AddRoleAssignmentsVersion")]
public partial class AddRoleAssignmentsVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "role_assignments_version",
            schema: "identity",
            table: "asp_net_users",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "role_assignments_version",
            schema: "identity",
            table: "asp_net_users");
    }
}
