using PANiXiDA.Core.Application.Authentication.Abstractions;

using PermissionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Permissions.Permissions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetPermissions;

public sealed class GetPermissionsHandler
    : IQueryHandler<GetPermissionsQuery, Result<IReadOnlyCollection<PermissionReadModel>>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AnyPermissions { get; } =
    [
        PermissionCatalog.IdentityRoles.Create.Code,
        PermissionCatalog.IdentityRoles.Update.Code,
        PermissionCatalog.IdentityUsers.Create.Code,
        PermissionCatalog.IdentityUsers.Update.Code
    ];

    public Task<Result<IReadOnlyCollection<PermissionReadModel>>> HandleAsync(
        GetPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<PermissionReadModel> permissions =
        [
            .. PermissionCatalog.All.Select(permission => new PermissionReadModel(
                Code: permission.Code,
                DisplayName: permission.DisplayName,
                ModuleCode: permission.ModuleCode,
                ModuleDisplayName: permission.ModuleDisplayName,
                ResourceCode: permission.ResourceCode,
                ResourceDisplayName: permission.ResourceDisplayName))
        ];

        return Task.FromResult(result: Result.Success(value: permissions));
    }
}
