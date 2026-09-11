using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Delete;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.Delete;

public sealed class DeleteHeroCommandValidatorTests
{
    [Fact(DisplayName = "Delete hero validator should reject an empty identifier when id is empty")]
    public void Validate_Should_ReturnError_When_IdIsEmpty()
    {
        var validator = new DeleteHeroCommandValidator();

        var result = validator.Validate(new DeleteHeroCommand(Guid.Empty));

        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(DeleteHeroCommand.Id));
    }
}
