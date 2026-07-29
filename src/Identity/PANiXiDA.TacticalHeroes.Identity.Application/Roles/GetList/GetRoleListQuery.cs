namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

public sealed record GetRoleListQuery(PaginationParameters Pagination)
    : IQuery<Result<PaginationResult<RoleListItemReadModel>>>;
