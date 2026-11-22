namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Units.GetList;

public sealed record UnitListItemResponse(
    Guid Id,
    string Name,
    Guid FactionId,
    string FactionName);
