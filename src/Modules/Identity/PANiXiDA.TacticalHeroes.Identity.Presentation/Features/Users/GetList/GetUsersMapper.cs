using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Users.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetUsersMapper
{
    [MapPropertyFromSource(nameof(GetUsersQuery.Filter))]
    internal static partial GetUsersQuery ToQuery(
        GetUsersRequest request,
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters);

    internal static partial PaginationResult<UserListItemResponse> ToResponse(
        PaginationResult<UserListItemReadModel> page);
}
