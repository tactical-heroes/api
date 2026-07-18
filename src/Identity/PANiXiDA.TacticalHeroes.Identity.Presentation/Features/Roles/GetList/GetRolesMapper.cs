using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetRolesMapper
{
    internal static partial PaginationResult<RoleListItemResponse> ToResponse(
        PaginationResult<RoleListItemReadModel> page);
}
