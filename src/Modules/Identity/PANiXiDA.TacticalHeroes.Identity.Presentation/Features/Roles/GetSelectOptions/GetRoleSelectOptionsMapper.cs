using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetSelectOptions;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Roles.GetSelectOptions;

[Mapper]
internal static partial class GetRoleSelectOptionsMapper
{
    internal static GetRoleSelectOptionsQuery ToQuery() => new();

    internal static partial IReadOnlyList<RoleSelectOptionResponse> ToResponse(
        IReadOnlyList<RoleSelectOptionReadModel> options);
}
