using PANiXiDA.TacticalHeroes.Compendium.Contracts;
using PANiXiDA.TacticalHeroes.Compendium.Contracts.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Contracts.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Internal;

internal sealed class CompendiumApi(IMediator mediator) : ICompendiumApi
{
    public async Task<UnitResponse?> GetUnitByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(CompendiumApiMapper.ToUnitQuery(id), cancellationToken);

        return result.IsSuccess ? CompendiumApiMapper.ToUnitResponse(result.Value) : null;
    }

    public async Task<HeroResponse?> GetHeroByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(CompendiumApiMapper.ToHeroQuery(id), cancellationToken);

        return result.IsSuccess ? CompendiumApiMapper.ToHeroResponse(result.Value) : null;
    }
}
