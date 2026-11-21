using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetInternalHero;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetInternalUnit;
using PANiXiDA.TacticalHeroes.Compendium.Contracts.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Contracts.Units;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Internal;

[Mapper]
internal static partial class CompendiumApiMapper
{
    internal static partial GetInternalUnitQuery ToUnitQuery(Guid id);
    internal static partial GetInternalHeroQuery ToHeroQuery(Guid id);
    internal static partial UnitResponse ToUnitResponse(UnitDetailsReadModel unit);
    internal static partial HeroResponse ToHeroResponse(HeroDetailsReadModel hero);
}
