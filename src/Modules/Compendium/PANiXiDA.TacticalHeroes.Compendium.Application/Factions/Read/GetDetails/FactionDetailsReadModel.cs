namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Read.GetDetails;

public sealed record FactionDetailsReadModel(
    Guid Id,
    string Name,
    string Description) : IReadModel;
