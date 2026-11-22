using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

public sealed class GetRoleDetailsHandler(IRolesReadRepository rolesReadRepository)
    : IQueryHandler<GetRoleDetailsQuery, Result<RoleDetailsReadModel>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Identity.Roles.Read.Code];

    public async Task<Result<RoleDetailsReadModel>> HandleAsync(
        GetRoleDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var role = await rolesReadRepository.GetDetailsByIdAsync(
            id: query.Id,
            cancellationToken: cancellationToken);

        return role is null
            ? Result.Failure<RoleDetailsReadModel>(
                error: Error.NotFound(message: "Role was not found."))
            : Result.Success(value: role);
    }
}
