using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetDetails;

internal static class GetFactionDetailsMapper
{
    internal static GetFactionDetailsResponse ToResponse(
        FactionDetailsReadModel faction)
    {
        return new GetFactionDetailsResponse(
            Id: faction.Id,
            Name: faction.Name,
            Description: faction.Description);
    }
}
