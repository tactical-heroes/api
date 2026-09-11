using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Write;

public sealed class UnitsRepository(
    CompendiumWriteDbContext dbContext,
    IAggregateTracker aggregateTracker)
    : EfRepository<CompendiumWriteDbContext, UnitId, Unit>(
        dbContext,
        aggregateTracker),
    IUnitsRepository;
