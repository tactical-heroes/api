using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed class GetRolesHandler(IRolesReadRepository rolesReadRepository)
    : IQueryHandler<GetRolesQuery, Result<PaginationResult<RoleListItemReadModel>>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Identity.Roles.Read.Code];

    public async Task<Result<PaginationResult<RoleListItemReadModel>>> HandleAsync(
        GetRolesQuery query,
        CancellationToken cancellationToken)
    {
        var roles = await rolesReadRepository.GetPageAsync(
            paginationParameters: query.PaginationParameters,
            sortingParameters: query.SortingParameters,
            cancellationToken: cancellationToken);

        return Result.Success(value: roles);
    }
}
