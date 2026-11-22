namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Read.GetList;

public sealed record UnitListItemReadModel(
    Guid Id,
    string Name,
    Guid FactionId,
    string FactionName) : IReadModel;
