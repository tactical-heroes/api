using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class KeepMatchedPlayersOccupied : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_match_searches_player_id",
                schema: "matchmaking",
                table: "match_searches");

            migrationBuilder.CreateIndex(
                name: "ix_match_searches_player_id",
                schema: "matchmaking",
                table: "match_searches",
                column: "player_id",
                unique: true,
                filter: "deleted_at IS NULL AND status IN ('Searching', 'Matched')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_match_searches_player_id",
                schema: "matchmaking",
                table: "match_searches");

            migrationBuilder.CreateIndex(
                name: "ix_match_searches_player_id",
                schema: "matchmaking",
                table: "match_searches",
                column: "player_id",
                unique: true,
                filter: "deleted_at IS NULL AND status = 'Searching'");
        }
    }
}
