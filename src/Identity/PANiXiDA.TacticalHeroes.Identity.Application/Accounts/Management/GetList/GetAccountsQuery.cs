namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetList;

public sealed record GetAccountsQuery(
    string? Email,
    PaginationParameters Pagination)
    : IQuery<Result<PaginationResult<AccountListItemReadModel>>>;
