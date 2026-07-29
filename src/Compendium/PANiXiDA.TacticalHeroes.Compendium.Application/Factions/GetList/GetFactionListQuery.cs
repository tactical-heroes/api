namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed record GetFactionListQuery(PaginationParameters Pagination)
    : IQuery<Result<PaginationResult<FactionListItemReadModel>>>;
