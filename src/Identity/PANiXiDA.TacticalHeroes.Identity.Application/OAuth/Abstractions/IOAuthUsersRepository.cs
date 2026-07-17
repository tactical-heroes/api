using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;

public interface IOAuthUsersRepository
{
    Task<Result<ExchangeTokenReadModel>> GetExchangeTokenByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken);

    Task<Result<UserInfoReadModel>> GetUserInfoByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken);
}
