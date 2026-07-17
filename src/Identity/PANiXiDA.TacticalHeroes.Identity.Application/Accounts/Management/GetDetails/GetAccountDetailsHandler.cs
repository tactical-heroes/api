using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;

public sealed class GetAccountDetailsHandler(IAccountsReadRepository accountsRepository)
    : IQueryHandler<GetAccountDetailsQuery, Result<AccountDetailsReadModel>>
{
    public Task<Result<AccountDetailsReadModel>> HandleAsync(
        GetAccountDetailsQuery query,
        CancellationToken cancellationToken)
    {
        return accountsRepository.GetDetailsByIdAsync(
            query.Id,
            cancellationToken);
    }
}
