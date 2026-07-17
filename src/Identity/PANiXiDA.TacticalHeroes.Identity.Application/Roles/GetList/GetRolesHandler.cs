using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed class GetRolesHandler(IRolesReadRepository rolesRepository)
    : IQueryHandler<GetRolesQuery, Result<PaginationResult<RoleListItemReadModel>>>
{
    public Task<Result<PaginationResult<RoleListItemReadModel>>> HandleAsync(
        GetRolesQuery query,
        CancellationToken cancellationToken)
    {
        return rolesRepository.GetPagedAsync(query.Pagination, cancellationToken);
    }
}
