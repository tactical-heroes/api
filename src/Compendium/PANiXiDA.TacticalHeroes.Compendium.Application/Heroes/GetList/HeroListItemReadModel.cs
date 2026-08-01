namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

public sealed record HeroListItemReadModel(
    Guid Id,
    string Name,
    Guid FactionId,
    string FactionName) : ReadModel;
