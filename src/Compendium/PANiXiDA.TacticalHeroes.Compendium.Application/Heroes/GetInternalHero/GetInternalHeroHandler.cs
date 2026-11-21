using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetInternalHero;

public sealed class GetInternalHeroHandler(IHeroesReadRepository heroesReadRepository)
    : IQueryHandler<GetInternalHeroQuery, Result<HeroDetailsReadModel>>
{
    public async Task<Result<HeroDetailsReadModel>> HandleAsync(
        GetInternalHeroQuery query,
        CancellationToken cancellationToken)
    {
        var hero = await heroesReadRepository.GetDetailsByIdAsync(query.Id, cancellationToken);

        return hero is null
            ? Result.Failure<HeroDetailsReadModel>(Error.NotFound("Hero was not found."))
            : Result.Success(hero);
    }
}
