using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Write;

public sealed class HeroesRepository(
    CompendiumWriteDbContext dbContext,
    IAggregateTracker aggregateTracker)
    : EfRepository<CompendiumWriteDbContext, HeroId, Hero>(
        dbContext,
        aggregateTracker),
    IHeroesRepository;
