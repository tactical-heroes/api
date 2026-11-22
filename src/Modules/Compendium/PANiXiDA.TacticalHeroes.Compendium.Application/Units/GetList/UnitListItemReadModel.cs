namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.GetList;

public sealed record UnitListItemReadModel(
    Guid Id,
    string Name,
    Guid FactionId,
    string FactionName) : IReadModel;
