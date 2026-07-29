using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;

public sealed class ExchangeUserTokenHandler(IOAuthUsersRepository usersRepository)
    : IQueryHandler<ExchangeUserTokenQuery, Result<ExchangeTokenReadModel>>
{
    public Task<Result<ExchangeTokenReadModel>> HandleAsync(
        ExchangeUserTokenQuery query,
        CancellationToken cancellationToken)
    {
        return usersRepository.GetExchangeTokenByUserIdAsync(
            query.UserId,
            cancellationToken);
    }
}
