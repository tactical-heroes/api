using PANiXiDA.TacticalHeroes.Battle.Application.ReadModels;
using PANiXiDA.TacticalHeroes.Compendium.Contracts.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Contracts.Units;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Clients;

[Mapper]
internal static partial class CompendiumClientMapper
{
    internal static partial UnitReadModel ToReadModel(UnitResponse unit);
    internal static partial HeroReadModel ToReadModel(HeroResponse hero);
}
