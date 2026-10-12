using PANiXiDA.TacticalHeroes.Identity.Application.Permissions.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Permissions.GetList;

[Mapper]
internal static partial class GetPermissionsMapper
{
    internal static GetPermissionsQuery ToQuery() => new();

    internal static partial IReadOnlyCollection<PermissionResponse> ToResponse(
        IReadOnlyCollection<PermissionReadModel> permissions);
}
