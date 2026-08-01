using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed class GetRolesHandler(IRolesReadRepository rolesReadRepository)
    : IQueryHandler<GetRolesQuery, Result<PaginationResult<RoleListItemReadModel>>>
{
    public async Task<Result<PaginationResult<RoleListItemReadModel>>> HandleAsync(
        GetRolesQuery query,
        CancellationToken cancellationToken)
    {
        var roles = await rolesReadRepository.GetPageAsync(
            pagination: query.Pagination,
            cancellationToken: cancellationToken);

        return Result.Success(value: roles);
    }
}
