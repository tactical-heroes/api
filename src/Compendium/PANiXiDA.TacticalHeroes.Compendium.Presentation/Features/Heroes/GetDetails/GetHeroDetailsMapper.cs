using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.GetDetails;

[Mapper]
internal static partial class GetHeroDetailsMapper
{
    internal static partial GetHeroDetailsQuery ToQuery(Guid id);

    internal static partial GetHeroDetailsResponse ToResponse(
        HeroDetailsReadModel hero);
}
