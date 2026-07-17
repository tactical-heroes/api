using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;

public sealed class ExchangeTokenHandler(IOAuthUsersRepository usersRepository)
    : IQueryHandler<ExchangeTokenQuery, Result<ExchangeTokenReadModel>>
{
    public Task<Result<ExchangeTokenReadModel>> HandleAsync(
        ExchangeTokenQuery query,
        CancellationToken cancellationToken)
    {
        return usersRepository.GetExchangeTokenByAccountIdAsync(
            query.AccountId,
            cancellationToken);
    }
}
