namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

public sealed record UnitListItemReadModel(
    Guid Id,
    string Name,
    Guid FactionId,
    string FactionName) : IReadModel;
