namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;

public sealed record GetHeroDetailsQuery(Guid Id)
    : IQuery<Result<HeroDetailsReadModel>>;
