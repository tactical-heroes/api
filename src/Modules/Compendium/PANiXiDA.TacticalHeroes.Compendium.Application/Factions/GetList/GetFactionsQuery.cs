namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed record GetFactionsQuery(
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<FactionListItemReadModel>>>;
