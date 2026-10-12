using PANiXiDA.Core.Application.Authentication.Abstractions;

using PermissionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Permissions.GetList;

public sealed class GetPermissionsHandler
    : IQueryHandler<GetPermissionsQuery, Result<IReadOnlyCollection<PermissionReadModel>>>, IRequireAuthorization
{
    public Task<Result<IReadOnlyCollection<PermissionReadModel>>> HandleAsync(
        GetPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<PermissionReadModel> permissions =
        [
            .. PermissionCatalog.All.Select(permission => new PermissionReadModel(
                Code: permission.Code,
                DisplayName: permission.DisplayName,
                Group: permission.Group))
        ];

        return Task.FromResult(result: Result.Success(value: permissions));
    }
}
