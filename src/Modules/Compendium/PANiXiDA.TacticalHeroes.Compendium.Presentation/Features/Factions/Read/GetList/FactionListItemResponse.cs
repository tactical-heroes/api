namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Read.GetList;

public sealed record FactionListItemResponse(
    Guid Id,
    string Name,
    string Description);
