namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetStatuses;

public sealed record GetAccountStatusesQuery()
    : IQuery<Result<IReadOnlyCollection<AccountStatusReadModel>>>;
