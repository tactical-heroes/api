namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed record GetFactionsQuery(PaginationParameters Pagination)
    : IQuery<Result<PaginationResult<FactionListItemReadModel>>>;
