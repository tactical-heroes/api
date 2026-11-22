using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20261020100000_RemoveNonPermissionClaims")]
public sealed class RemoveNonPermissionClaims : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM identity.asp_net_user_claims
            WHERE lower(btrim(claim_type)) IS DISTINCT FROM 'permission';

            DELETE FROM identity.asp_net_role_claims
            WHERE lower(btrim(claim_type)) IS DISTINCT FROM 'permission';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Deleted assignments cannot be reconstructed; rollback leaves permissions unchanged.
    }
}
