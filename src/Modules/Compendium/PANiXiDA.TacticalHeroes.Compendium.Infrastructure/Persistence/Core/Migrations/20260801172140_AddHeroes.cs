using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "heroes",
                schema: "compendium",
                columns: table => new
                {
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    morale = table.Column<int>(type: "integer", nullable: false),
                    luck = table.Column<int>(type: "integer", nullable: false),
                    faction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stats_attack = table.Column<int>(type: "integer", nullable: false),
                    stats_defense = table.Column<int>(type: "integer", nullable: false),
                    stats_initiative = table.Column<double>(type: "double precision", nullable: false),
                    stats_maximum_damage = table.Column<int>(type: "integer", nullable: false),
                    stats_minimum_damage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_heroes", x => x.id);
                    table.ForeignKey(
                        name: "fk_heroes_factions_faction_id",
                        column: x => x.faction_id,
                        principalSchema: "compendium",
                        principalTable: "factions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_heroes_faction_id",
                schema: "compendium",
                table: "heroes",
                column: "faction_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "heroes",
                schema: "compendium");
        }
    }
}
