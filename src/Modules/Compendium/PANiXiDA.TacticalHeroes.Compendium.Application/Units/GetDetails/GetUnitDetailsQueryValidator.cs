using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;

public sealed class GetUnitDetailsQueryValidator
    : AbstractValidator<GetUnitDetailsQuery>
{
    public GetUnitDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(UnitId.Create);
    }
}
