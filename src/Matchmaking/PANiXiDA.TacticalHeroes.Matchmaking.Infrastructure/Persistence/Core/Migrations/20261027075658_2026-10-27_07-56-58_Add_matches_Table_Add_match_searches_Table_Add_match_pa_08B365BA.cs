using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class _20261027_075658_Add_matches_Table_Add_match_searches_Table_Add_match_pa_08B365BA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "matches",
                schema: "matchmaking",
                columns: table => new
                {
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    matched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_matches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "match_searches",
                schema: "matchmaking",
                columns: table => new
                {
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    build_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    queued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_searches", x => x.id);
                    table.ForeignKey(
                        name: "fk_match_searches_match_match_id",
                        column: x => x.match_id,
                        principalSchema: "matchmaking",
                        principalTable: "matches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "match_participants",
                schema: "matchmaking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_search_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    build_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_participants", x => x.id);
                    table.ForeignKey(
                        name: "fk_match_participants_match_match_id",
                        column: x => x.match_id,
                        principalSchema: "matchmaking",
                        principalTable: "matches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_match_participants_match_searches_match_search_id",
                        column: x => x.match_search_id,
                        principalSchema: "matchmaking",
                        principalTable: "match_searches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_match_participants_match_id_player_id",
                schema: "matchmaking",
                table: "match_participants",
                columns: new[] { "match_id", "player_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_match_participants_match_search_id",
                schema: "matchmaking",
                table: "match_participants",
                column: "match_search_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_match_searches_match_id",
                schema: "matchmaking",
                table: "match_searches",
                column: "match_id");

            migrationBuilder.CreateIndex(
                name: "ix_match_searches_mode_status_rating_queued_at",
                schema: "matchmaking",
                table: "match_searches",
                columns: new[] { "mode", "status", "rating", "queued_at" });

            migrationBuilder.CreateIndex(
                name: "ix_match_searches_player_id",
                schema: "matchmaking",
                table: "match_searches",
                column: "player_id",
                unique: true,
                filter: "deleted_at IS NULL AND status IN ('Searching', 'Reserved')");

            migrationBuilder.CreateIndex(
                name: "ix_match_searches_status_expires_at",
                schema: "matchmaking",
                table: "match_searches",
                columns: new[] { "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_matches_game_id",
                schema: "matchmaking",
                table: "matches",
                column: "game_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_matches_status_creation_deadline",
                schema: "matchmaking",
                table: "matches",
                columns: new[] { "status", "creation_deadline" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_participants",
                schema: "matchmaking");

            migrationBuilder.DropTable(
                name: "match_searches",
                schema: "matchmaking");

            migrationBuilder.DropTable(
                name: "matches",
                schema: "matchmaking");
        }
    }
}
