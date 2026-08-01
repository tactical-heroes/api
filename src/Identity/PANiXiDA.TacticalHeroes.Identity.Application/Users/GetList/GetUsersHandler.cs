using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed class GetUsersHandler(IUsersReadRepository usersReadRepository)
    : IQueryHandler<GetUsersQuery, Result<PaginationResult<UserListItemReadModel>>>
{
    public async Task<Result<PaginationResult<UserListItemReadModel>>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var users = await usersReadRepository.GetPageAsync(
            email: query.Email,
            pagination: query.Pagination,
            cancellationToken: cancellationToken);

        return Result.Success(value: users);
    }
}
