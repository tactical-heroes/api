using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

public interface IFactionsRepository
{
    Task<Faction?> GetByIdAsync(
        FactionId id,
        CancellationToken cancellationToken);

    Task AddAsync(
        Faction aggregateRoot,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Faction aggregateRoot,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Faction aggregateRoot,
        CancellationToken cancellationToken);
}
