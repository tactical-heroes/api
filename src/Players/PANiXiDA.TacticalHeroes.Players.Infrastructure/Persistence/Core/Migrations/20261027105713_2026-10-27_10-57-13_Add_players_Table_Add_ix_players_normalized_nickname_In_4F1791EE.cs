using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class _20261027_105713_Add_players_Table_Add_ix_players_normalized_nickname_In_4F1791EE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "players",
                schema: "players",
                columns: table => new
                {
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nickname = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    avatar_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    normalized_nickname = table.Column<string>(type: "text", nullable: false, computedColumnSql: "upper(nickname)", stored: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_players", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_players_normalized_nickname",
                schema: "players",
                table: "players",
                column: "normalized_nickname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_players_user_id",
                schema: "players",
                table: "players",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "players",
                schema: "players");
        }
    }
}
