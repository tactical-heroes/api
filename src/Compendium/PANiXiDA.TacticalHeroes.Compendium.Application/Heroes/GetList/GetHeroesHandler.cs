using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

public sealed class GetHeroesHandler(IHeroesReadRepository heroesReadRepository)
    : IQueryHandler<GetHeroesQuery, Result<PaginationResult<HeroListItemReadModel>>>
{
    public async Task<Result<PaginationResult<HeroListItemReadModel>>> HandleAsync(
        GetHeroesQuery query,
        CancellationToken cancellationToken)
    {
        var heroes = await heroesReadRepository.GetPageAsync(
            pagination: query.Pagination,
            cancellationToken: cancellationToken);

        return Result.Success(value: heroes);
    }
}
