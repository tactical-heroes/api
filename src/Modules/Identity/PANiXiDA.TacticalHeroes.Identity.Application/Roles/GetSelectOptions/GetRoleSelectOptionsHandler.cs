using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetSelectOptions;

public sealed class GetRoleSelectOptionsHandler(IRolesReadRepository rolesReadRepository) :
    IQueryHandler<GetRoleSelectOptionsQuery, Result<IReadOnlyList<RoleSelectOptionReadModel>>>,
    IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Identity.Roles.Read.Code];

    public async Task<Result<IReadOnlyList<RoleSelectOptionReadModel>>> HandleAsync(
        GetRoleSelectOptionsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RoleSelectOptionReadModel> options =
            await rolesReadRepository.GetSelectOptionsAsync(cancellationToken);

        return Result.Success(value: options);
    }
}
