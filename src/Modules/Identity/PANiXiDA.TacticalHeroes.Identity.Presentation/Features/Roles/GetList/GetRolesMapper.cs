using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Roles.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetRolesMapper
{
    [MapPropertyFromSource(nameof(GetRolesQuery.PaginationParameters))]
    internal static partial GetRolesQuery ToQuery(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters);

    internal static partial PaginationResult<RoleListItemResponse> ToResponse(
        PaginationResult<RoleListItemReadModel> page);
}
