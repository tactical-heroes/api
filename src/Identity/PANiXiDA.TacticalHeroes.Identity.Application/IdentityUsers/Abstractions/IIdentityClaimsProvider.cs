using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;

namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;

public interface IIdentityClaimsProvider
{
    Task<IdentityClaims> GetClaimsAsync(
        IdentityUser user,
        CancellationToken cancellationToken);
}
