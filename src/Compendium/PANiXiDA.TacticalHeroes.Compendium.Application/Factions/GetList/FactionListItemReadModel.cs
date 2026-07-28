namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed record FactionListItemReadModel(
    Guid Id,
    string Name,
    string Description);
