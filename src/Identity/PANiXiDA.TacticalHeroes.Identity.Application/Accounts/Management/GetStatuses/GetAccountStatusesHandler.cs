using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetStatuses;

public sealed class GetAccountStatusesHandler(IAccountsReadRepository accountsRepository)
    : IQueryHandler<GetAccountStatusesQuery, Result<IReadOnlyCollection<AccountStatusReadModel>>>
{
    public Task<Result<IReadOnlyCollection<AccountStatusReadModel>>> HandleAsync(
        GetAccountStatusesQuery query,
        CancellationToken cancellationToken)
    {
        return accountsRepository.GetStatusesAsync(cancellationToken);
    }
}
