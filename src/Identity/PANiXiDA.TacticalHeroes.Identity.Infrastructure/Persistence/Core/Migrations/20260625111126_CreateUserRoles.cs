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
            name: "user_roles",
            schema: "identity",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "pk_user_roles",
                    x => new { x.user_id, x.role_id });
                table.ForeignKey(
                    name: "fk_user_roles_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: "identity",
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_user_roles_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "identity",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_user_roles_role_id",
            schema: "identity",
            table: "user_roles",
            column: "role_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "user_roles",
            schema: "identity");
    }
}
