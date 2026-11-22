using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20260717150000_AddUserStatus")]
public partial class AddUserStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "user_name",
            schema: "identity",
            table: "asp_net_users",
            type: "character varying(256)",
            maxLength: 256,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(320)",
            oldMaxLength: 320);

        migrationBuilder.AlterColumn<string>(
            name: "normalized_user_name",
            schema: "identity",
            table: "asp_net_users",
            type: "character varying(256)",
            maxLength: 256,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(320)",
            oldMaxLength: 320);

        migrationBuilder.AddColumn<string>(
            name: "status",
            schema: "identity",
            table: "asp_net_users",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "Active");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "status",
            schema: "identity",
            table: "asp_net_users");

        migrationBuilder.AlterColumn<string>(
            name: "user_name",
            schema: "identity",
            table: "asp_net_users",
            type: "character varying(320)",
            maxLength: 320,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(256)",
            oldMaxLength: 256);

        migrationBuilder.AlterColumn<string>(
            name: "normalized_user_name",
            schema: "identity",
            table: "asp_net_users",
            type: "character varying(320)",
            maxLength: 320,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(256)",
            oldMaxLength: 256);
    }
}
