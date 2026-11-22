using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class _20261027_081555_Add_match_searches_Table_Add_ix_match_searches_mode_sta_18ECF4D4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_searches", x => x.id);
                });

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
                filter: "deleted_at IS NULL AND status = 'Searching'");

            migrationBuilder.CreateIndex(
                name: "ix_match_searches_status_expires_at",
                schema: "matchmaking",
                table: "match_searches",
                columns: new[] { "status", "expires_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_searches",
                schema: "matchmaking");
        }
    }
}
