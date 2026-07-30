namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;

public sealed record FactionDetailsReadModel(
    Guid Id,
    string Name,
    string Description) : ReadModel;
