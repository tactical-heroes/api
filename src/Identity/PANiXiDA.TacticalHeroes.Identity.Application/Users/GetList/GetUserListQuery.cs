namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed record GetUserListQuery(
    string? Email,
    PaginationParameters Pagination)
    : IQuery<Result<PaginationResult<UserListItemReadModel>>>;
