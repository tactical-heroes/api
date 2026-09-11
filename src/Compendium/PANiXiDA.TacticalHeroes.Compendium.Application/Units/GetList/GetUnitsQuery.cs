namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

public sealed record GetUnitsQuery(PaginationParameters Pagination)
    : IQuery<Result<PaginationResult<UnitListItemReadModel>>>;
