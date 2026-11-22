using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Common.Filters;

public sealed class RolesFilterValidatorTests
{
    [Fact(DisplayName = "Validate should accept filter when no criteria are specified")]
    public void Validate_Should_AcceptFilter_When_NoCriteriaAreSpecified()
    {
        var validator = new RolesFilterValidator();

        var result = validator.Validate(new RolesFilter());

        result.IsValid.ShouldBeTrue();
    }
}
