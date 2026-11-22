using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;

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
