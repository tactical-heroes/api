using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Heroes.Write;

public sealed class HeroesRepository(
    CompendiumWriteDbContext dbContext)
    :
        EfRepository<CompendiumWriteDbContext, HeroId, Hero>(
            dbContext),
        IHeroesRepository;
