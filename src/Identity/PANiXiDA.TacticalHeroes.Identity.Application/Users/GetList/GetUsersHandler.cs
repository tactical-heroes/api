using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed class GetUsersHandler(IUsersReadRepository usersReadRepository)
    : IQueryHandler<GetUsersQuery, Result<PaginationResult<UserListItemReadModel>>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } =
    [
        Permissions.IdentityUsers.Read.Code
    ];

    public async Task<Result<PaginationResult<UserListItemReadModel>>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var users = await usersReadRepository.GetPageAsync(
            filter: query.Filter,
            paginationParameters: query.PaginationParameters,
            sortingParameters: query.SortingParameters,
            cancellationToken: cancellationToken);

        return Result.Success(value: users);
    }
}
