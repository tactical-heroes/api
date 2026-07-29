using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed class GetUserListHandler(IUsersReadRepository usersReadRepository)
    : IQueryHandler<GetUserListQuery, Result<PaginationResult<UserListItemReadModel>>>
{
    public async Task<Result<PaginationResult<UserListItemReadModel>>> HandleAsync(
        GetUserListQuery query,
        CancellationToken cancellationToken)
    {
        var users = await usersReadRepository.GetPagedAsync(
            email: query.Email,
            pagination: query.Pagination,
            cancellationToken: cancellationToken);

        return Result.Success(value: users);
    }
}
