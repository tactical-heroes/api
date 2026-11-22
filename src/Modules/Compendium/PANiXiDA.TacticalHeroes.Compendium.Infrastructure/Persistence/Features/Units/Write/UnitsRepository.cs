using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Units.Write;

public sealed class UnitsRepository(
    CompendiumWriteDbContext dbContext)
    :
        EfRepository<CompendiumWriteDbContext, UnitId, Unit>(
            dbContext),
        IUnitsRepository;
