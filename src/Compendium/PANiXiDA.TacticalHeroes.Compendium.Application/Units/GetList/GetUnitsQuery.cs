namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

public sealed record GetUnitsQuery(
    PaginationParameters Pagination,
    SortingParameters Sorting)
    : IQuery<Result<PaginationResult<UnitListItemReadModel>>>;
