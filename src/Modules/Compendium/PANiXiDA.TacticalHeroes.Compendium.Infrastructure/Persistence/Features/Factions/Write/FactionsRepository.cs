using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Write;

public sealed class FactionsRepository(
    CompendiumWriteDbContext dbContext)
    :
        EfRepository<CompendiumWriteDbContext, FactionId, Faction>(
            dbContext),
        IFactionsRepository;
