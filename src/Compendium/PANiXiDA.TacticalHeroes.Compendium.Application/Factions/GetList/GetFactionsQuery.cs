namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed record GetFactionsQuery(
    PaginationParameters Pagination,
    SortingParameters Sorting)
    : IQuery<Result<PaginationResult<FactionListItemReadModel>>>;
