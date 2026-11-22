namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Heroes.GetList;

public sealed record GetHeroesQuery(
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<HeroListItemReadModel>>>;
