namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

public sealed record GetHeroesQuery(
    PaginationParameters Pagination,
    SortingParameters Sorting)
    : IQuery<Result<PaginationResult<HeroListItemReadModel>>>;
