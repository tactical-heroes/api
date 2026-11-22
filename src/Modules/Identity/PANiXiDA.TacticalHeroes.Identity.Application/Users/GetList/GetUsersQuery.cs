using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetList;

public sealed record GetUsersQuery(
    UsersFilter Filter,
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<UserListItemReadModel>>>;
