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
            name: "identity_user_claims",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                identity_user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_identity_user_claims", x => x.id);
                table.ForeignKey(
                    name: "fk_identity_user_claims_identity_users_identity_user_id",
                    column: x => x.identity_user_id,
                    principalTable: "identity_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_identity_user_claims_identity_user_id_type_value",
            table: "identity_user_claims",
            columns: new[] { "identity_user_id", "type", "value" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "identity_user_claims");
    }
}
