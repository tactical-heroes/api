namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetList;

public sealed record GetRolesQuery(
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<RoleListItemReadModel>>>;
