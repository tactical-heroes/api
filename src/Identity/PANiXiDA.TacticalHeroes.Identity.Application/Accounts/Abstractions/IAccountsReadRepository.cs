using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;

public interface IAccountsReadRepository
{
    Task<Result<PaginationResult<AccountListItemReadModel>>> GetPagedAsync(
        string? email,
        PaginationParameters pagination,
        CancellationToken cancellationToken);

    Task<Result<AccountDetailsReadModel>> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
