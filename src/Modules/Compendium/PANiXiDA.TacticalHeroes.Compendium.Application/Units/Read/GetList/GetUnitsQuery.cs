namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Read.GetList;

public sealed record GetUnitsQuery(
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<UnitListItemReadModel>>>;
