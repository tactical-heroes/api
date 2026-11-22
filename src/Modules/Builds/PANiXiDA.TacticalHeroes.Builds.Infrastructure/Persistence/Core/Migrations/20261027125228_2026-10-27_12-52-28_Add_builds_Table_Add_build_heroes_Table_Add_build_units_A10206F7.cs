using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class _20261027_125228_Add_builds_Table_Add_build_heroes_Table_Add_build_units_A10206F7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "builds",
                schema: "builds",
                columns: table => new
                {
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_builds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "build_heroes",
                schema: "builds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    build_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_build_heroes", x => x.id);
                    table.ForeignKey(
                        name: "fk_build_heroes_builds_build_id",
                        column: x => x.build_id,
                        principalSchema: "builds",
                        principalTable: "builds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "build_units",
                schema: "builds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    build_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_build_units", x => x.id);
                    table.ForeignKey(
                        name: "fk_build_units_builds_build_id",
                        column: x => x.build_id,
                        principalSchema: "builds",
                        principalTable: "builds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_build_heroes_build_id_hero_id",
                schema: "builds",
                table: "build_heroes",
                columns: new[] { "build_id", "hero_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_build_units_build_id_unit_id",
                schema: "builds",
                table: "build_units",
                columns: new[] { "build_id", "unit_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_builds_player_id",
                schema: "builds",
                table: "builds",
                column: "player_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "build_heroes",
                schema: "builds");

            migrationBuilder.DropTable(
                name: "build_units",
                schema: "builds");

            migrationBuilder.DropTable(
                name: "builds",
                schema: "builds");
        }
    }
}
