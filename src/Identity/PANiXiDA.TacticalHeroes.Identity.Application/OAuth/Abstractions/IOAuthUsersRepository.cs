using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;

public interface IOAuthUsersRepository
{
    Task<Result<ExchangeTokenReadModel>> GetExchangeTokenByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Result<UserInfoReadModel>> GetUserInfoByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
