using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;

public sealed class GetHeroDetailsQueryValidator
    : AbstractValidator<GetHeroDetailsQuery>
{
    public GetHeroDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(HeroId.Create);
    }
}
