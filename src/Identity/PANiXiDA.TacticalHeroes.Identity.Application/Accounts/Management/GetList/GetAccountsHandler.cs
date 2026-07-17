using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetList;

public sealed class GetAccountsHandler(IAccountsReadRepository accountsRepository)
    : IQueryHandler<GetAccountsQuery, Result<PaginationResult<AccountListItemReadModel>>>
{
    public Task<Result<PaginationResult<AccountListItemReadModel>>> HandleAsync(
        GetAccountsQuery query,
        CancellationToken cancellationToken)
    {
        return accountsRepository.GetPagedAsync(
            query.Email,
            query.Pagination,
            cancellationToken);
    }
}
