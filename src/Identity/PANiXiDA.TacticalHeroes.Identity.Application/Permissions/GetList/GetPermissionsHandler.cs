using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Common.Authentication;

using PermissionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Permissions.GetList;

public sealed class GetPermissionsHandler
    : IQueryHandler<GetPermissionsQuery, Result<IReadOnlyCollection<PermissionReadModel>>>, IRequireAuthorization
{
    public Task<Result<IReadOnlyCollection<PermissionReadModel>>> HandleAsync(
        GetPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        var sections = Sections.All.ToDictionary(section => section.Code, StringComparer.Ordinal);
        var resources = PermissionCatalog.Resources.ToDictionary(resource => (resource.SectionCode, resource.Code));

        IReadOnlyCollection<PermissionReadModel> permissions =
        [
            .. PermissionCatalog.All.Select(permission => new PermissionReadModel(
                Code: permission.Code,
                DisplayName: permission.DisplayName,
                SectionCode: permission.SectionCode,
                SectionDisplayName: sections[permission.SectionCode].DisplayName,
                ResourceCode: permission.ResourceCode,
                ResourceDisplayName: resources[(permission.SectionCode, permission.ResourceCode)].DisplayName))
        ];

        return Task.FromResult(result: Result.Success(value: permissions));
    }
}
