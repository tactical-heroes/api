using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class _20261026_112056_Add_match_assignments_Table_Add_matchmaking_tickets_Tab_88000E94 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "match_assignments",
                schema: "matchmaking",
                columns: table => new
                {
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    proposed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    acceptance_deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: true),
                    closure_reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_assignments", x => x.id);
                    table.CheckConstraint("ck_assignment_game", "(status = 'Ready') = (game_id IS NOT NULL)");
                    table.CheckConstraint("ck_assignment_window", "acceptance_deadline > proposed_at AND creation_deadline > acceptance_deadline");
                });

            migrationBuilder.CreateTable(
                name: "matchmaking_tickets",
                schema: "matchmaking",
                columns: table => new
                {
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    build_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    queued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    match_assignment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_matchmaking_tickets", x => x.id);
                    table.CheckConstraint("ck_ticket_assignment", "status NOT IN ('Reserved', 'Assigned') OR match_assignment_id IS NOT NULL");
                    table.CheckConstraint("ck_ticket_window", "expires_at > queued_at");
                    table.ForeignKey(
                        name: "fk_matchmaking_tickets_match_assignments_match_assignment_id",
                        column: x => x.match_assignment_id,
                        principalSchema: "matchmaking",
                        principalTable: "match_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "match_participants",
                schema: "matchmaking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    build_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_participants", x => x.id);
                    table.ForeignKey(
                        name: "fk_match_participants_match_assignment_match_assignment_id",
                        column: x => x.match_assignment_id,
                        principalSchema: "matchmaking",
                        principalTable: "match_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_match_participants_matchmaking_ticket_ticket_id",
                        column: x => x.ticket_id,
                        principalSchema: "matchmaking",
                        principalTable: "matchmaking_tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_match_assignments_game_id",
                schema: "matchmaking",
                table: "match_assignments",
                column: "game_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_match_assignments_status_acceptance_deadline",
                schema: "matchmaking",
                table: "match_assignments",
                columns: new[] { "status", "acceptance_deadline" });

            migrationBuilder.CreateIndex(
                name: "ix_match_assignments_status_creation_deadline",
                schema: "matchmaking",
                table: "match_assignments",
                columns: new[] { "status", "creation_deadline" });

            migrationBuilder.CreateIndex(
                name: "ix_match_participants_match_assignment_id_player_id",
                schema: "matchmaking",
                table: "match_participants",
                columns: new[] { "match_assignment_id", "player_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_match_participants_ticket_id",
                schema: "matchmaking",
                table: "match_participants",
                column: "ticket_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_matchmaking_tickets_match_assignment_id",
                schema: "matchmaking",
                table: "matchmaking_tickets",
                column: "match_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_matchmaking_tickets_mode_status_rating_queued_at",
                schema: "matchmaking",
                table: "matchmaking_tickets",
                columns: new[] { "mode", "status", "rating", "queued_at" });

            migrationBuilder.CreateIndex(
                name: "ix_matchmaking_tickets_player_id",
                schema: "matchmaking",
                table: "matchmaking_tickets",
                column: "player_id",
                unique: true,
                filter: "deleted_at IS NULL AND status IN ('Searching', 'Reserved')");

            migrationBuilder.CreateIndex(
                name: "ix_matchmaking_tickets_status_expires_at",
                schema: "matchmaking",
                table: "matchmaking_tickets",
                columns: new[] { "status", "expires_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_participants",
                schema: "matchmaking");

            migrationBuilder.DropTable(
                name: "matchmaking_tickets",
                schema: "matchmaking");

            migrationBuilder.DropTable(
                name: "match_assignments",
                schema: "matchmaking");
        }
    }
}
