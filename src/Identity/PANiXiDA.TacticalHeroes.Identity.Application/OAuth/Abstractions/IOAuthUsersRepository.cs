using PANiXiDA.TacticalHeroes.Identity.Application.Users.ExchangeToken;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetUserInfo;

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
