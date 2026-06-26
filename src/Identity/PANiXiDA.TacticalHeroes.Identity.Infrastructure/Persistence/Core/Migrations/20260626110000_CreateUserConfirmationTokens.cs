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
            name: "user_confirmation_tokens",
            schema: "identity",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_confirmation_tokens", x => x.user_id);
                table.ForeignKey(
                    name: "fk_user_confirmation_tokens_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "identity",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_user_confirmation_tokens_expires_at_utc",
            schema: "identity",
            table: "user_confirmation_tokens",
            column: "expires_at_utc");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "user_confirmation_tokens",
            schema: "identity");
    }
}
