using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.Common.Filters;

public sealed class UnitsFilterValidatorTests
{
    [Fact(DisplayName = "Validate should accept filter when no criteria are specified")]
    public void Validate_Should_AcceptFilter_When_NoCriteriaAreSpecified()
    {
        var validator = new UnitsFilterValidator();

        var result = validator.Validate(new UnitsFilter());

        result.IsValid.ShouldBeTrue();
    }
}
