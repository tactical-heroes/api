using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetInternalUnit;

public sealed class GetInternalUnitQueryValidator : AbstractValidator<GetInternalUnitQuery>
{
    public GetInternalUnitQueryValidator()
    {
        RuleFor(query => query.Id).MustBeValidDomainValue(UnitId.Create);
    }
}
