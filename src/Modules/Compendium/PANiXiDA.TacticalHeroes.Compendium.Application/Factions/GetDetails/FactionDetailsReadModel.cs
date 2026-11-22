namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetDetails;

public sealed record FactionDetailsReadModel(
    Guid Id,
    string Name,
    string Description) : IReadModel;
