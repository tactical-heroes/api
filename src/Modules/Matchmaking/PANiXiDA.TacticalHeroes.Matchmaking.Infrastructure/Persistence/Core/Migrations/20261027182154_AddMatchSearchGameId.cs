using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchSearchGameId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "game_id",
                schema: "matchmaking",
                table: "match_searches",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_match_searches_game_id",
                schema: "matchmaking",
                table: "match_searches",
                column: "game_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_match_searches_game_id",
                schema: "matchmaking",
                table: "match_searches");

            migrationBuilder.DropColumn(
                name: "game_id",
                schema: "matchmaking",
                table: "match_searches");
        }
    }
}
