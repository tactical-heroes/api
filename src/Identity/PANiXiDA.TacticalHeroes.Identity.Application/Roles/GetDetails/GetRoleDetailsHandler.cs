using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

public sealed class GetRoleDetailsHandler(IRolesReadRepository rolesRepository)
    : IQueryHandler<GetRoleDetailsQuery, Result<RoleDetailsReadModel>>
{
    public Task<Result<RoleDetailsReadModel>> HandleAsync(
        GetRoleDetailsQuery query,
        CancellationToken cancellationToken)
    {
        return rolesRepository.GetDetailsByIdAsync(query.Id, cancellationToken);
    }
}
