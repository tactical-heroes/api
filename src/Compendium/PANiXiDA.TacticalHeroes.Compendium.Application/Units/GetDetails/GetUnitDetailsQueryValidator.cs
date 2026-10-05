using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;

public sealed class GetUnitDetailsQueryValidator
    : AbstractValidator<GetUnitDetailsQuery>
{
    public GetUnitDetailsQueryValidator()
    {
        RuleFor(expression: query => query.Id)
            .MustBeValidDomainValue(factory: UnitId.Create);
    }
}
