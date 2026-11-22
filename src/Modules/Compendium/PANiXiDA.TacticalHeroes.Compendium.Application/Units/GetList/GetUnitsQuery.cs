namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.GetList;

public sealed record GetUnitsQuery(
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<UnitListItemReadModel>>>;
