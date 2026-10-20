using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20261020100000_RemoveSectionClaims")]
public sealed class RemoveSectionClaims : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM identity.asp_net_user_claims
            WHERE lower(btrim(claim_type)) = 'section';

            DELETE FROM identity.asp_net_role_claims
            WHERE lower(btrim(claim_type)) = 'section';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Deleted assignments cannot be reconstructed; rollback leaves permissions unchanged.
    }
}
