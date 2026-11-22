using PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.ExchangeToken;
using PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.Abstractions;

public interface IOAuthUsersRepository
{
    Task<Result> CheckAvailabilityByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Result<ExchangeTokenReadModel>> GetExchangeTokenByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Result<UserInfoReadModel>> GetUserInfoByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
