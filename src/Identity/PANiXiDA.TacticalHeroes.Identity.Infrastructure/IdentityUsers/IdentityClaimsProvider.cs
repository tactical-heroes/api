using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityUsers;

public sealed class IdentityClaimsProvider(
    IIdentityRolesRepository identityRolesRepository)
    : IIdentityClaimsProvider
{
    public async Task<IdentityClaims> GetClaimsAsync(
        IdentityUser user,
        CancellationToken cancellationToken)
    {
        var roleIds = user.Roles
            .Select(role => role.RoleId)
            .Distinct()
            .ToArray();

        var roles = await identityRolesRepository.GetByIdsAsync(
            roleIds,
            cancellationToken);

        var roleNames = roles
            .Select(role => role.Name.Value)
            .ToArray();

        var rolePermissionNames = roles
            .SelectMany(role => role.Permissions.Select(permission => permission.Name.Value));

        var directPermissionNames = user.DirectPermissions
            .Select(permission => permission.Name.Value);

        return new IdentityClaims(
            roleNames
                .Order(StringComparer.Ordinal)
                .ToArray(),
            directPermissionNames
                .Concat(rolePermissionNames)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }
}
