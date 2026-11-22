using PANiXiDA.TacticalHeroes.Battle.Application.Battles.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Application.Battles.Common.Filters;

public sealed class BattlesFilterValidatorTests
{
    [Fact(DisplayName = "Battles filter validator should accept empty filter when no criteria are specified")]
    public void Validate_Should_AcceptEmptyFilter_When_NoCriteriaAreSpecified()
    {
        var validator = new BattlesFilterValidator();
        var filter = new BattlesFilter();

        var result = validator.Validate(filter);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
}
