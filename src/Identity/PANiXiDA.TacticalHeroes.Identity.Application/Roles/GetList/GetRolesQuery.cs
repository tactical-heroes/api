namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed record GetRolesQuery(PaginationParameters Pagination)
    : IQuery<Result<PaginationResult<RoleListItemReadModel>>>;
