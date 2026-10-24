using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class _20261024_192312_Add_battles_Table_Add_units_Table_Add_ix_units_battle_i_A478DA5A : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "battles",
                schema: "battle",
                columns: table => new
                {
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_battles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "units",
                schema: "battle",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    battle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    damage_range_maximum = table.Column<int>(type: "integer", nullable: false),
                    damage_range_minimum = table.Column<int>(type: "integer", nullable: false),
                    stack_state_count = table.Column<int>(type: "integer", nullable: false),
                    stack_state_current_unit_health = table.Column<int>(type: "integer", nullable: false),
                    stats_attack = table.Column<int>(type: "integer", nullable: false),
                    stats_defense = table.Column<int>(type: "integer", nullable: false),
                    stats_max_health = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_units", x => x.id);
                    table.ForeignKey(
                        name: "fk_units_battles_battle_id",
                        column: x => x.battle_id,
                        principalSchema: "battle",
                        principalTable: "battles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_units_battle_id",
                schema: "battle",
                table: "units",
                column: "battle_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "units",
                schema: "battle");

            migrationBuilder.DropTable(
                name: "battles",
                schema: "battle");
        }
    }
}
