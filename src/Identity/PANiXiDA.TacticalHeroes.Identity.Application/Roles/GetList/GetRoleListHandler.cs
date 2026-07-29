using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed class GetRoleListHandler(IRolesReadRepository rolesReadRepository)
    : IQueryHandler<GetRoleListQuery, Result<PaginationResult<RoleListItemReadModel>>>
{
    public async Task<Result<PaginationResult<RoleListItemReadModel>>> HandleAsync(
        GetRoleListQuery query,
        CancellationToken cancellationToken)
    {
        var roles = await rolesReadRepository.GetPagedAsync(
            pagination: query.Pagination,
            cancellationToken: cancellationToken);

        return Result.Success(value: roles);
    }
}
