using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260625111125_CreateUserClaims")]
public partial class CreateUserClaims : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "user_claims",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_claims", x => x.id);
                table.ForeignKey(
                    name: "fk_user_claims_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "identity",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_user_claims_user_id_type_value",
            schema: "identity",
            table: "user_claims",
            columns: new[] { "user_id", "type", "value" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "user_claims",
            schema: "identity");
    }
}
