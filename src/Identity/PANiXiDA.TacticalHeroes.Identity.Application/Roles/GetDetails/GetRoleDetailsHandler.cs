using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

public sealed class GetRoleDetailsHandler(IRolesReadRepository rolesRepository)
    : IQueryHandler<GetRoleDetailsQuery, Result<RoleDetailsReadModel>>
{
    public async Task<Result<RoleDetailsReadModel>> HandleAsync(
        GetRoleDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var role = await rolesRepository.GetDetailsByIdAsync(
            id: query.Id,
            cancellationToken: cancellationToken);

        return role is null
            ? Result.Failure<RoleDetailsReadModel>(
                error: Error.NotFound(message: "Role was not found."))
            : Result.Success(value: role);
    }
}
