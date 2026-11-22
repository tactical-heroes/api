using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.GetDetails;

public sealed class GetUnitDetailsQueryValidatorTests
{
    [Fact(DisplayName = "Unit details validator should reject an empty identifier when id is empty")]
    public void Validate_Should_ReturnError_When_IdIsEmpty()
    {
        var validator = new GetUnitDetailsQueryValidator();

        var result = validator.Validate(new GetUnitDetailsQuery(Guid.Empty));

        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(GetUnitDetailsQuery.Id));
    }
}
