using PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetSections;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Claims.GetSections;

public sealed class GetSectionsQueryValidatorTests
{
    [Fact(DisplayName = "Sections validator should accept a query when query is valid")]
    public void Validate_Should_ReturnSuccess_When_QueryIsValid()
    {
        var validator = new GetSectionsQueryValidator();

        var result = validator.Validate(new GetSectionsQuery());

        result.IsValid.ShouldBeTrue();
    }
}
