using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetDetails;

[Mapper]
internal static partial class GetFactionDetailsMapper
{
    internal static partial GetFactionDetailsResponse ToResponse(
        FactionDetailsReadModel faction);
}
