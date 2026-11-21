using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetInternalUnit;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.GetInternalUnit;

public sealed class GetInternalUnitQueryValidatorTests
{
    [Fact(DisplayName = "Unit details validator should reject an empty identifier when id is empty")]
    public void Validate_Should_ReturnError_When_IdIsEmpty()
    {
        var validator = new GetInternalUnitQueryValidator();

        var result = validator.Validate(new GetInternalUnitQuery(Guid.Empty));

        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(GetInternalUnitQuery.Id));
    }
}
