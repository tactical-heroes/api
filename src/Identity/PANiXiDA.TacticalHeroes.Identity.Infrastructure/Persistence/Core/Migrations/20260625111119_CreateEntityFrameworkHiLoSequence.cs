using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260625111119_CreateEntityFrameworkHiLoSequence")]
public partial class CreateEntityFrameworkHiLoSequence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "identity");

        migrationBuilder.CreateSequence(
            name: "EntityFrameworkHiLoSequence",
            schema: "identity",
            incrementBy: 10);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropSequence(
            name: "EntityFrameworkHiLoSequence",
            schema: "identity");
    }
}
