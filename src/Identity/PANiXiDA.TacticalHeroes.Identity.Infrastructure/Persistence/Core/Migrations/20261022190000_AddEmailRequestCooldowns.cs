using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20261022190000_AddEmailRequestCooldowns")]
public partial class AddEmailRequestCooldowns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "email_request_cooldowns",
            schema: "identity",
            columns: table => new
            {
                normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                purpose = table.Column<int>(type: "integer", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey(
                name: "pk_email_request_cooldowns",
                columns: cooldown => new { cooldown.normalized_email, cooldown.purpose }));

        migrationBuilder.CreateIndex(
            name: "ix_email_request_cooldowns_expires_at",
            schema: "identity",
            table: "email_request_cooldowns",
            column: "expires_at");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "email_request_cooldowns", schema: "identity");
    }
}
