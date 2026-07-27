using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class _20260726_154512_EnsureSchema_CreateSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "compendium");

            migrationBuilder.CreateSequence(
                name: "EntityFrameworkHiLoSequence",
                schema: "compendium",
                incrementBy: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "EntityFrameworkHiLoSequence",
                schema: "compendium");
        }
    }
}
