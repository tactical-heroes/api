using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetUsersMapper
{
    internal static partial PaginationResult<UserListItemResponse> ToResponse(
        PaginationResult<UserListItemReadModel> page);
}
