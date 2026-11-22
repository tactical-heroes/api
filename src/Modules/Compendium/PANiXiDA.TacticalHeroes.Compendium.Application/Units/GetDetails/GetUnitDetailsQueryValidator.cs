using PANiXiDA.TacticalHeroes.Modules.Compendium.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.GetDetails;

public sealed class GetUnitDetailsQueryValidator
    : AbstractValidator<GetUnitDetailsQuery>
{
    public GetUnitDetailsQueryValidator()
    {
        RuleFor(query => query.Id)
            .MustBeValidDomainValue(UnitId.Create);
    }
}
