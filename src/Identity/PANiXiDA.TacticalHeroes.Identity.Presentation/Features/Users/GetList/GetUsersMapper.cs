using PANiXiDA.TacticalHeroes.Identity.Application.Users.Common.Filters;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetUsersMapper
{
    internal static GetUsersQuery ToQuery(
        GetUsersRequest request,
        PaginationParameters pagination,
        SortingParameters sorting)
    {
        return new GetUsersQuery(
            Filter: new UsersFilter(request.Email),
            Pagination: pagination,
            Sorting: sorting);
    }

    internal static partial PaginationResult<UserListItemResponse> ToResponse(
        PaginationResult<UserListItemReadModel> page);
}
