using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class SplitUnitRangedAttack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "stats_ranged_attack_range",
                schema: "compendium",
                table: "units",
                newName: "ranged_attack_ranged_attack_range");

            migrationBuilder.RenameColumn(
                name: "stats_shots",
                schema: "compendium",
                table: "units",
                newName: "ranged_attack_shots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ranged_attack_ranged_attack_range",
                schema: "compendium",
                table: "units",
                newName: "stats_ranged_attack_range");

            migrationBuilder.RenameColumn(
                name: "ranged_attack_shots",
                schema: "compendium",
                table: "units",
                newName: "stats_shots");
        }
    }
}
