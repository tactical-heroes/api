using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;

public sealed class GetFactionDetailsQueryValidator
    : AbstractValidator<GetFactionDetailsQuery>
{
    public GetFactionDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(FactionId.Create);
    }
}
