using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class PrepareBattlesFromGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "definition_id",
                schema: "battle",
                table: "units",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "faction_id",
                schema: "battle",
                table: "units",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "owner_player_id",
                schema: "battle",
                table: "units",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "traits_initiative",
                schema: "battle",
                table: "units",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "traits_luck",
                schema: "battle",
                table: "units",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "traits_morale",
                schema: "battle",
                table: "units",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "traits_ranged_attack_range",
                schema: "battle",
                table: "units",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "traits_shots",
                schema: "battle",
                table: "units",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "traits_speed",
                schema: "battle",
                table: "units",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "first_player_id",
                schema: "battle",
                table: "battles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "game_id",
                schema: "battle",
                table: "battles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "second_player_id",
                schema: "battle",
                table: "battles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "battle",
                table: "battles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "InProgress");

            migrationBuilder.CreateTable(
                name: "heroes",
                schema: "battle",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    battle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    faction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    stats_attack = table.Column<int>(type: "integer", nullable: false),
                    stats_defense = table.Column<int>(type: "integer", nullable: false),
                    stats_initiative = table.Column<double>(type: "double precision", nullable: false),
                    stats_luck = table.Column<int>(type: "integer", nullable: false),
                    stats_maximum_damage = table.Column<int>(type: "integer", nullable: false),
                    stats_minimum_damage = table.Column<int>(type: "integer", nullable: false),
                    stats_morale = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_heroes", x => x.id);
                    table.ForeignKey(
                        name: "fk_heroes_battles_battle_id",
                        column: x => x.battle_id,
                        principalSchema: "battle",
                        principalTable: "battles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_battles_game_id",
                schema: "battle",
                table: "battles",
                column: "game_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_heroes_battle_id",
                schema: "battle",
                table: "heroes",
                column: "battle_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "heroes",
                schema: "battle");

            migrationBuilder.DropIndex(
                name: "ix_battles_game_id",
                schema: "battle",
                table: "battles");

            migrationBuilder.DropColumn(
                name: "definition_id",
                schema: "battle",
                table: "units");

            migrationBuilder.DropColumn(
                name: "faction_id",
                schema: "battle",
                table: "units");

            migrationBuilder.DropColumn(
                name: "owner_player_id",
                schema: "battle",
                table: "units");

            migrationBuilder.DropColumn(
                name: "traits_initiative",
                schema: "battle",
                table: "units");

            migrationBuilder.DropColumn(
                name: "traits_luck",
                schema: "battle",
                table: "units");

            migrationBuilder.DropColumn(
                name: "traits_morale",
                schema: "battle",
                table: "units");

            migrationBuilder.DropColumn(
                name: "traits_ranged_attack_range",
                schema: "battle",
                table: "units");

            migrationBuilder.DropColumn(
                name: "traits_shots",
                schema: "battle",
                table: "units");

            migrationBuilder.DropColumn(
                name: "traits_speed",
                schema: "battle",
                table: "units");

            migrationBuilder.DropColumn(
                name: "first_player_id",
                schema: "battle",
                table: "battles");

            migrationBuilder.DropColumn(
                name: "game_id",
                schema: "battle",
                table: "battles");

            migrationBuilder.DropColumn(
                name: "second_player_id",
                schema: "battle",
                table: "battles");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "battle",
                table: "battles");
        }
    }
}
