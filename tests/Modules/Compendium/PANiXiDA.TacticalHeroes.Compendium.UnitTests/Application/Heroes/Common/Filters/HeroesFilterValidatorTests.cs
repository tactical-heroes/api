using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.Common.Filters;

public sealed class HeroesFilterValidatorTests
{
    [Fact(DisplayName = "Validate should accept filter when no criteria are specified")]
    public void Validate_Should_AcceptFilter_When_NoCriteriaAreSpecified()
    {
        var validator = new HeroesFilterValidator();

        var result = validator.Validate(new HeroesFilter());

        result.IsValid.ShouldBeTrue();
    }
}
