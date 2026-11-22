using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetUserTokenPrincipal;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;

public interface IOAuthUsersRepository
{
    Task<Result> CheckAvailabilityByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Result<OAuthUserTokenPrincipalReadModel>> GetTokenPrincipalByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Result<UserInfoReadModel>> GetUserInfoByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
