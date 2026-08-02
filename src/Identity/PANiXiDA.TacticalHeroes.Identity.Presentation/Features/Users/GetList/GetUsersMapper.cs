using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetUsersMapper
{
    internal static partial GetUsersQuery ToQuery(
        GetUsersRequest request,
        PaginationParameters pagination);

    internal static partial PaginationResult<UserListItemResponse> ToResponse(
        PaginationResult<UserListItemReadModel> page);
}
