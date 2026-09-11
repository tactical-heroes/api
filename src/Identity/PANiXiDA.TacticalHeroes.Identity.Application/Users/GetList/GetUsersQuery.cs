using PANiXiDA.TacticalHeroes.Identity.Application.Users.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed record GetUsersQuery(
    UsersFilter Filter,
    PaginationParameters Pagination)
    : IQuery<Result<PaginationResult<UserListItemReadModel>>>;
