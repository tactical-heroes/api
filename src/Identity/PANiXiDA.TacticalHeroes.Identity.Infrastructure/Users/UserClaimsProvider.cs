using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Users;

public sealed class UserClaimsProvider(
    IRolesRepository rolesRepository)
    : IUserClaimsProvider
{
    public async Task<UserClaims> GetClaimsAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var roleIds = user.Roles
            .Select(role => role.RoleId)
            .Distinct()
            .ToArray();

        var roles = await rolesRepository.GetByIdsAsync(
            roleIds,
            cancellationToken);

        var roleNames = roles
            .Select(role => role.Name.Value)
            .ToArray();

        var roleClaims = roles
            .SelectMany(role => role.Claims.Select(claim => new AuthorizationClaim(
                claim.Type.Value,
                claim.Value.Value)));

        var directClaims = user.Claims
            .Select(claim => new AuthorizationClaim(
                claim.Type.Value,
                claim.Value.Value));

        return new UserClaims(
            roleNames
                .Order(StringComparer.Ordinal)
                .ToArray(),
            directClaims
                .Concat(roleClaims)
                .Distinct()
                .OrderBy(claim => claim.Type, StringComparer.Ordinal)
                .ThenBy(claim => claim.Value, StringComparer.Ordinal)
                .ToArray());
    }
}
