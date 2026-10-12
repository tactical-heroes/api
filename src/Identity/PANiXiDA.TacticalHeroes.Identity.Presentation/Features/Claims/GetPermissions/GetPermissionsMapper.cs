using PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetPermissions;

using Riok.Mapperly.Abstractions;

using ApplicationClaimTypes = PANiXiDA.Core.Application.Authentication.ClaimTypes;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims.GetPermissions;

[Mapper]
internal static partial class GetPermissionsMapper
{
    internal static GetPermissionsQuery ToQuery() => new();

    internal static GetPermissionsResponse ToResponse(IReadOnlyCollection<PermissionReadModel> permissions) =>
        new(ApplicationClaimTypes.Permission, ToItems(permissions));

    private static partial IReadOnlyCollection<PermissionResponse> ToItems(
        IReadOnlyCollection<PermissionReadModel> permissions);
}
