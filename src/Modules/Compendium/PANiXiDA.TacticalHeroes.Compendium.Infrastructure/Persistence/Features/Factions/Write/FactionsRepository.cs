using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Factions.Write;

public sealed class FactionsRepository(
    CompendiumWriteDbContext dbContext)
    :
        EfRepository<CompendiumWriteDbContext, FactionId, Faction>(
            dbContext),
        IFactionsRepository;
