namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Factions.GetDetails;

public sealed record GetFactionDetailsResponse(
    Guid Id,
    string Name,
    string Description);
