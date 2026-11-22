namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Read.GetList;

public sealed record FactionListItemReadModel(
    Guid Id,
    string Name,
    string Description) : IReadModel;
