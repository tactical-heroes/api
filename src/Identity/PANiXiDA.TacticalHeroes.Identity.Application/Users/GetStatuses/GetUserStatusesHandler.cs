using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetStatuses;

public sealed class GetUserStatusesHandler
    : IQueryHandler<GetUserStatusesQuery, Result<IReadOnlyCollection<UserStatusReadModel>>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Identity.Users.Read.Code];

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
