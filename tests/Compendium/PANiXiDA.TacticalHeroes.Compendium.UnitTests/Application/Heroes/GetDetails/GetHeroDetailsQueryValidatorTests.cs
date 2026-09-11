using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.GetDetails;

public sealed class GetHeroDetailsQueryValidatorTests
{
    [Fact(DisplayName = "Hero details validator should reject an empty identifier when id is empty")]
    public void Validate_Should_ReturnError_When_IdIsEmpty()
    {
        var validator = new GetHeroDetailsQueryValidator();

        var result = validator.Validate(new GetHeroDetailsQuery(Guid.Empty));

        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(GetHeroDetailsQuery.Id));
    }
}
