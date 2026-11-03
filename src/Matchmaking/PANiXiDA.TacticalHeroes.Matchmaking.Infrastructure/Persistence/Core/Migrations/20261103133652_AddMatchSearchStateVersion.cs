using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchSearchStateVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "match_search_state_versions",
                schema: "matchmaking");

            migrationBuilder.AddColumn<long>(
                name: "state_version",
                schema: "matchmaking",
                table: "match_searches",
                type: "bigint",
                nullable: false,
                defaultValueSql: "nextval('matchmaking.match_search_state_versions')");

            migrationBuilder.Sql("""
                WITH ordered_searches AS (
                    SELECT id, row_number() OVER (
                        ORDER BY CASE WHEN status IN ('Searching', 'Matched') THEN 1 ELSE 0 END,
                            queued_at, created_at, id) AS version
                    FROM matchmaking.match_searches
                )
                UPDATE matchmaking.match_searches AS searches
                SET state_version = ordered_searches.version
                FROM ordered_searches
                WHERE searches.id = ordered_searches.id;

                SELECT setval('matchmaking.match_search_state_versions',
                    COALESCE((SELECT MAX(state_version) FROM matchmaking.match_searches), 1),
                    EXISTS(SELECT 1 FROM matchmaking.match_searches));
                """);

            migrationBuilder.CreateIndex(
                name: "ix_match_searches_player_id_state_version",
                schema: "matchmaking",
                table: "match_searches",
                columns: new[] { "player_id", "state_version" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_match_searches_player_id_state_version",
                schema: "matchmaking",
                table: "match_searches");

            migrationBuilder.DropColumn(
                name: "state_version",
                schema: "matchmaking",
                table: "match_searches");

            migrationBuilder.DropSequence(
                name: "match_search_state_versions",
                schema: "matchmaking");
        }
    }
}
