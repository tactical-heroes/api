using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260625111126_CreateUserRoles")]
public partial class CreateUserRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "identity_user_roles",
            columns: table => new
            {
                identity_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                identity_user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "pk_identity_user_roles",
                    x => new { x.identity_user_id, x.identity_role_id });
                table.ForeignKey(
                    name: "fk_identity_user_roles_identity_roles_identity_role_id",
                    column: x => x.identity_role_id,
                    principalTable: "identity_roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_identity_user_roles_identity_users_identity_user_id",
                    column: x => x.identity_user_id,
                    principalTable: "identity_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_identity_user_roles_identity_role_id",
            table: "identity_user_roles",
            column: "identity_role_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "identity_user_roles");
    }
}
