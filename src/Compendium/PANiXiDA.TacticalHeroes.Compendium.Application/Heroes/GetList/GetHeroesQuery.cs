namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

public sealed record GetHeroesQuery(PaginationParameters Pagination)
    : IQuery<Result<PaginationResult<HeroListItemReadModel>>>;
