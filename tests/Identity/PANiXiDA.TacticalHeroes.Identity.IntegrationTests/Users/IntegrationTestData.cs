using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Users;

internal static class IntegrationTestData
{
    internal static User CreateUser(
        Guid id,
        string email,
        bool isConfirmed,
        IEnumerable<Guid> roleIds,
        IEnumerable<(string Type, string Value)> claims)
    {
        return User.Create(
            id: UserId.Create(value: id).Value,
            email: Email.Create(value: email).Value,
            confirmationStatus: UserConfirmationStatus.From(isConfirmed: isConfirmed),
            roleIds: roleIds.Select(roleId => RoleId.Create(value: roleId).Value),
            claims: claims.Select(claim => UserClaim.Create(
                type: ClaimType.Create(value: claim.Type).Value,
                value: ClaimValue.Create(value: claim.Value).Value)));
    }
}
