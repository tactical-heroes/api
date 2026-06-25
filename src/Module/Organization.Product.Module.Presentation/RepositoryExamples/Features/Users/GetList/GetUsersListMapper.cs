using Organization.Product.Module.Application.Users;
using Organization.Product.Module.Application.Users.GetList;

using Riok.Mapperly.Abstractions;

namespace Organization.Product.Module.Presentation.Features.Users.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetUsersListMapper
{
    internal static partial UsersFilterParameters ToFilterParameters(
        GetUsersListRequest request);

    internal static partial PaginationResult<UserListItemResponse> ToResponse(
        PaginationResult<UserListItemReadModel> pagedResult);
}
