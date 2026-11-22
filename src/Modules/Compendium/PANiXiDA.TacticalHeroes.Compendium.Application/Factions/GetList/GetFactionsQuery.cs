namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetList;

public sealed record GetFactionsQuery(
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<FactionListItemReadModel>>>;
