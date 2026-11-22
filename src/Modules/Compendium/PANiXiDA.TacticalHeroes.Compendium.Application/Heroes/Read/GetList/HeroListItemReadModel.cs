namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Read.GetList;

public sealed record HeroListItemReadModel(
    Guid Id,
    string Name,
    Guid FactionId,
    string FactionName) : IReadModel;
