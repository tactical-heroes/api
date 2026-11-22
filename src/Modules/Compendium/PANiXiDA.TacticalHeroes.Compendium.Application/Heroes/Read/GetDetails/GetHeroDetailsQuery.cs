namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Read.GetDetails;

public sealed record GetHeroDetailsQuery(Guid Id)
    : IQuery<Result<HeroDetailsReadModel>>;
