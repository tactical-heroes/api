namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Read.GetList;

public sealed record GetRolesQuery(
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<RoleListItemReadModel>>>;
