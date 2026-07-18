namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed record GetUsersQuery(
    string? Email,
    PaginationParameters Pagination)
    : IQuery<Result<PaginationResult<UserListItemReadModel>>>;
