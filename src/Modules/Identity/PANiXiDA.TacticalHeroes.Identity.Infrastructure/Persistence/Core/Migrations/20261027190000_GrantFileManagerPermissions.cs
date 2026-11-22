using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20261027190000_GrantFileManagerPermissions")]
public sealed class GrantFileManagerPermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO identity.asp_net_role_claims (role_id, claim_type, claim_value)
            SELECT role.id, 'permission', seed.claim_value
            FROM (VALUES
                    ('DEVELOPER', 'file-manager.personal-files.create.own'),
                    ('DEVELOPER', 'file-manager.personal-files.create.all'),
                    ('DEVELOPER', 'file-manager.personal-files.read.own'),
                    ('DEVELOPER', 'file-manager.personal-files.read.all'),
                    ('DEVELOPER', 'file-manager.personal-files.update.own'),
                    ('DEVELOPER', 'file-manager.personal-files.update.all'),
                    ('DEVELOPER', 'file-manager.personal-files.delete.own'),
                    ('DEVELOPER', 'file-manager.personal-files.delete.all'),
                    ('DEVELOPER', 'file-manager.avatars.create'),
                    ('DEVELOPER', 'file-manager.avatars.read'),
                    ('DEVELOPER', 'file-manager.avatars.update'),
                    ('DEVELOPER', 'file-manager.avatars.delete'),
                    ('PLAYER', 'file-manager.personal-files.create.own'),
                    ('PLAYER', 'file-manager.personal-files.read.own'),
                    ('PLAYER', 'file-manager.personal-files.update.own'),
                    ('PLAYER', 'file-manager.personal-files.delete.own'),
                    ('PLAYER', 'file-manager.avatars.read')
            ) AS seed(normalized_name, claim_value)
            JOIN identity.asp_net_roles AS role ON role.normalized_name = seed.normalized_name
            ON CONFLICT (role_id, claim_type, claim_value) DO NOTHING;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Assignments may have been edited, so rollback preserves the granted permissions.
    }
}
