using Organization.Product.Module.Application.Users.Abstractions;

namespace Organization.Product.Module.Application.Users.GetList;

public sealed class GetUsersListHandler(IUsersReadRepository usersQueryService)
    : IQueryHandler<GetUsersListQuery, Result<PaginationResult<UserListItemReadModel>>>
{
    public async Task<Result<PaginationResult<UserListItemReadModel>>> HandleAsync(
        GetUsersListQuery query,
        CancellationToken cancellationToken)
    {
        var users = await usersQueryService.GetPagedListAsync(
            query.FilterParameters,
            query.PaginationParameters,
            query.SortParameters,
            cancellationToken);

        return Result.Success(users);
    }
}
