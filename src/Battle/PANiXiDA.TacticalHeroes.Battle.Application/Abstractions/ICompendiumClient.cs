using PANiXiDA.TacticalHeroes.Battle.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Battle.Application.Abstractions;

public interface ICompendiumClient
{
    Task<UnitReadModel?> GetUnitByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<HeroReadModel?> GetHeroByIdAsync(Guid id, CancellationToken cancellationToken);
}
