using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed class GetUsersHandler(IUsersReadRepository usersRepository)
    : IQueryHandler<GetUsersQuery, Result<PaginationResult<UserListItemReadModel>>>
{
    public Task<Result<PaginationResult<UserListItemReadModel>>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        return usersRepository.GetPagedAsync(
            query.Email,
            query.Pagination,
            cancellationToken);
    }
}
