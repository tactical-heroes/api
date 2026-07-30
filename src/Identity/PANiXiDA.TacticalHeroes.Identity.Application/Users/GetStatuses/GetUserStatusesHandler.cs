using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetStatuses;

public sealed class GetUserStatusesHandler
    : IQueryHandler<GetUserStatusesQuery, Result<IReadOnlyCollection<UserStatusReadModel>>>
{
    public Task<Result<IReadOnlyCollection<UserStatusReadModel>>> HandleAsync(
        GetUserStatusesQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<UserStatusReadModel> statuses =
        [
            .. UserStatus.GetAll().Select(status =>
                new UserStatusReadModel(
                    Id: status.Id,
                    Name: status.Name,
                    DisplayName: status.DisplayName))
        ];

        return Task.FromResult(result: Result.Success(value: statuses));
    }
}
