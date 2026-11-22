using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;

[DbContext(typeof(IdentityWriteDbContext))]
[Migration("20261018100100_SeedRolesAndClaims")]
public partial class SeedRolesAndClaims : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO identity.asp_net_roles
                (id, name, normalized_name, concurrency_stamp, created_at, updated_at, is_default)
            SELECT id, name, normalized_name, id::text, NOW(), NOW(), false
            FROM (VALUES
                    ('0199f2a0-0000-7000-8000-000000000001'::uuid, 'Developer', 'DEVELOPER'),
                    ('0199f2a0-0000-7000-8000-000000000002'::uuid, 'Moderator', 'MODERATOR'),
                    ('0199f2a0-0000-7000-8000-000000000003'::uuid, 'Player', 'PLAYER')
            ) AS roles(id, name, normalized_name)
            ON CONFLICT (normalized_name) DO NOTHING;

            UPDATE identity.asp_net_roles
            SET is_default = true
            WHERE normalized_name = 'PLAYER'
                AND NOT EXISTS (SELECT 1 FROM identity.asp_net_roles WHERE is_default);

            INSERT INTO identity.asp_net_role_claims (role_id, claim_type, claim_value)
            SELECT role.id, seed.claim_type, seed.claim_value
            FROM (VALUES
                    ('DEVELOPER', 'permission', 'identity.users.create'),
                    ('DEVELOPER', 'permission', 'identity.users.read'),
                    ('DEVELOPER', 'permission', 'identity.users.update'),
                    ('DEVELOPER', 'permission', 'identity.users.delete'),
                    ('DEVELOPER', 'permission', 'identity.roles.create'),
                    ('DEVELOPER', 'permission', 'identity.roles.read'),
                    ('DEVELOPER', 'permission', 'identity.roles.update'),
                    ('DEVELOPER', 'permission', 'identity.roles.delete'),
                    ('DEVELOPER', 'permission', 'compendium.factions.create'),
                    ('DEVELOPER', 'permission', 'compendium.factions.read'),
                    ('DEVELOPER', 'permission', 'compendium.factions.update'),
                    ('DEVELOPER', 'permission', 'compendium.factions.delete'),
                    ('DEVELOPER', 'permission', 'compendium.heroes.create'),
                    ('DEVELOPER', 'permission', 'compendium.heroes.read'),
                    ('DEVELOPER', 'permission', 'compendium.heroes.update'),
                    ('DEVELOPER', 'permission', 'compendium.heroes.delete'),
                    ('DEVELOPER', 'permission', 'compendium.units.create'),
                    ('DEVELOPER', 'permission', 'compendium.units.read'),
                    ('DEVELOPER', 'permission', 'compendium.units.update'),
                    ('DEVELOPER', 'permission', 'compendium.units.delete'),
                    ('DEVELOPER', 'section', 'identity'),
                    ('DEVELOPER', 'section', 'compendium'),
                    ('MODERATOR', 'permission', 'compendium.factions.create'),
                    ('MODERATOR', 'permission', 'compendium.factions.read'),
                    ('MODERATOR', 'permission', 'compendium.factions.update'),
                    ('MODERATOR', 'permission', 'compendium.factions.delete'),
                    ('MODERATOR', 'permission', 'compendium.heroes.create'),
                    ('MODERATOR', 'permission', 'compendium.heroes.read'),
                    ('MODERATOR', 'permission', 'compendium.heroes.update'),
                    ('MODERATOR', 'permission', 'compendium.heroes.delete'),
                    ('MODERATOR', 'permission', 'compendium.units.create'),
                    ('MODERATOR', 'permission', 'compendium.units.read'),
                    ('MODERATOR', 'permission', 'compendium.units.update'),
                    ('MODERATOR', 'permission', 'compendium.units.delete'),
                    ('MODERATOR', 'section', 'compendium')
            ) AS seed(normalized_name, claim_type, claim_value)
            JOIN identity.asp_net_roles AS role ON role.normalized_name = seed.normalized_name
            ON CONFLICT (role_id, claim_type, claim_value) DO NOTHING;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Roles may have been edited or assigned to users, so rollback preserves the seeded data.
    }
}
