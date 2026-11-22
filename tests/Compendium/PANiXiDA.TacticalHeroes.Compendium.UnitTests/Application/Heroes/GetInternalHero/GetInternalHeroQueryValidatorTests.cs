using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetInternalHero;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.GetInternalHero;

public sealed class GetInternalHeroQueryValidatorTests
{
    [Fact(DisplayName = "Hero details validator should reject an empty identifier when id is empty")]
    public void Validate_Should_ReturnError_When_IdIsEmpty()
    {
        var validator = new GetInternalHeroQueryValidator();

        var result = validator.Validate(new GetInternalHeroQuery(Guid.Empty));

        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(GetInternalHeroQuery.Id));
    }
}
