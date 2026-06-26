using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260626110000_CreateUserConfirmationTokens")]
public partial class CreateUserConfirmationTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "identity_user_confirmation_tokens",
            columns: table => new
            {
                identity_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_identity_user_confirmation_tokens", x => x.identity_user_id);
                table.ForeignKey(
                    name: "fk_identity_user_confirmation_tokens_identity_users_identity_user_id",
                    column: x => x.identity_user_id,
                    principalTable: "identity_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_identity_user_confirmation_tokens_expires_at_utc",
            table: "identity_user_confirmation_tokens",
            column: "expires_at_utc");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "identity_user_confirmation_tokens");
    }
}
