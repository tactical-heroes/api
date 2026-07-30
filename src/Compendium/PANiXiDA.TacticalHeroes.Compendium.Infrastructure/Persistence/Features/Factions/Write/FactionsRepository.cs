using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Write;

public sealed class FactionsRepository(
    CompendiumWriteDbContext dbContext,
    IAggregateTracker aggregateTracker)
    : EfRepository<CompendiumWriteDbContext, FactionId, Faction>(
        dbContext: dbContext,
        aggregateTracker: aggregateTracker),
    IFactionsRepository;
