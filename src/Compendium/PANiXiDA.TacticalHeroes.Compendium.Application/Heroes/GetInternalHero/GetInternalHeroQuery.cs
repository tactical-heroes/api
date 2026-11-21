using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetInternalHero;

public sealed record GetInternalHeroQuery(Guid Id) : IQuery<Result<HeroDetailsReadModel>>;
