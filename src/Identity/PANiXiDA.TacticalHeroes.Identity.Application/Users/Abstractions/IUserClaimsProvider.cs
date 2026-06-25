using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUserClaimsProvider
{
    Task<UserClaims> GetClaimsAsync(
        User user,
        CancellationToken cancellationToken);
}
