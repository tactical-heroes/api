namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Heroes.GetList;

public sealed record HeroListItemResponse(
    Guid Id,
    string Name,
    Guid FactionId,
    string FactionName);
