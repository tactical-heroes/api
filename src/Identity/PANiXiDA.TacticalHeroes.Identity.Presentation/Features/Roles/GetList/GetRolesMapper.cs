using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetRolesMapper
{
    internal static GetRolesQuery ToQuery(
        PaginationParameters pagination,
        SortingParameters sorting)
    {
        return new GetRolesQuery(pagination, sorting);
    }

    internal static partial PaginationResult<RoleListItemResponse> ToResponse(
        PaginationResult<RoleListItemReadModel> page);
}
