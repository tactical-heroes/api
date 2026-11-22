using PANiXiDA.TacticalHeroes.Compendium.Contracts.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Contracts.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.Contracts;

public interface ICompendiumApi
{
    Task<UnitResponse?> GetUnitByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<HeroResponse?> GetHeroByIdAsync(Guid id, CancellationToken cancellationToken);
}
