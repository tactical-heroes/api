namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed record GetRolesQuery(
    PaginationParameters Pagination,
    SortingParameters Sorting)
    : IQuery<Result<PaginationResult<RoleListItemReadModel>>>;
