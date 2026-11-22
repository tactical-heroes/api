using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetInternalHero;

public sealed class GetInternalHeroQueryValidator : AbstractValidator<GetInternalHeroQuery>
{
    public GetInternalHeroQueryValidator()
    {
        RuleFor(query => query.Id).MustBeValidDomainValue(HeroId.Create);
    }
}
