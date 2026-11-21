using PANiXiDA.TacticalHeroes.Battle.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Application.ReadModels;
using PANiXiDA.TacticalHeroes.Compendium.Contracts;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Clients;

public sealed class CompendiumClient(ICompendiumApi compendiumApi) : ICompendiumClient
{
    public async Task<UnitReadModel?> GetUnitByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var unit = await compendiumApi.GetUnitByIdAsync(id, cancellationToken);

        return unit is null ? null : CompendiumClientMapper.ToReadModel(unit);
    }

    public async Task<HeroReadModel?> GetHeroByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var hero = await compendiumApi.GetHeroByIdAsync(id, cancellationToken);

        return hero is null ? null : CompendiumClientMapper.ToReadModel(hero);
    }
}
