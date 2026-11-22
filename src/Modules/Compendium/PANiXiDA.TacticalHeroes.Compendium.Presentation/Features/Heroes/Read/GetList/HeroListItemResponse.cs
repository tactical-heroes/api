namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Read.GetList;

public sealed record HeroListItemResponse(
    Guid Id,
    string Name,
    Guid FactionId,
    string FactionName);
