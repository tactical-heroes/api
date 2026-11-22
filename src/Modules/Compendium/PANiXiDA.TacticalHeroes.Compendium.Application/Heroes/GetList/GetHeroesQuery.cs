namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

public sealed record GetHeroesQuery(
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<HeroListItemReadModel>>>;
