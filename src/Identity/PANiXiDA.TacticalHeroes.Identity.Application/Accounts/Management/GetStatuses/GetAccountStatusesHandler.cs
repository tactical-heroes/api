using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetStatuses;

public sealed class GetAccountStatusesHandler
    : IQueryHandler<GetAccountStatusesQuery, Result<IReadOnlyCollection<AccountStatusReadModel>>>
{
    public Task<Result<IReadOnlyCollection<AccountStatusReadModel>>> HandleAsync(
        GetAccountStatusesQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<AccountStatusReadModel> statuses =
        [
            .. AccountStatus.GetAll().Select(status =>
                new AccountStatusReadModel(
                    Id: status.Id,
                    Name: status.Name,
                    DisplayName: status.DisplayName))
        ];

        return Task.FromResult(result: Result.Success(value: statuses));
    }
}
