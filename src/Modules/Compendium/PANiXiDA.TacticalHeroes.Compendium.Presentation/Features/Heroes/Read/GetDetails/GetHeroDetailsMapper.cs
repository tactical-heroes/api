using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Read.GetDetails;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Read.GetDetails;

[Mapper]
internal static partial class GetHeroDetailsMapper
{
    internal static partial GetHeroDetailsQuery ToQuery(Guid id);

    internal static partial GetHeroDetailsResponse ToResponse(
        HeroDetailsReadModel hero);
}
