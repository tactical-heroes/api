using PANiXiDA.TacticalHeroes.Common.Authentication;
using PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetPermissions;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims.GetPermissions;

[Mapper]
internal static partial class GetPermissionsMapper
{
    internal static GetPermissionsQuery ToQuery() => new();

    internal static GetPermissionsResponse ToResponse(IReadOnlyCollection<PermissionReadModel> permissions) =>
        new(CustomClaimTypes.Permission, ToItems(permissions));

    private static partial IReadOnlyCollection<PermissionResponse> ToItems(
        IReadOnlyCollection<PermissionReadModel> permissions);
}
