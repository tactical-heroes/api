namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Read.GetDetails;

public sealed record GetFactionDetailsResponse(
    Guid Id,
    string Name,
    string Description);
