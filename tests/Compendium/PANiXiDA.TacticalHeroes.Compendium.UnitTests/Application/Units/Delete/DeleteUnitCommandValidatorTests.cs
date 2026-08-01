using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Delete;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.Delete;

public sealed class DeleteUnitCommandValidatorTests
{
    [Fact(DisplayName = "Delete unit validator should reject an empty identifier when id is empty")]
    public void Validate_Should_ReturnError_When_IdIsEmpty()
    {
        var validator = new DeleteUnitCommandValidator();

        var result = validator.Validate(new DeleteUnitCommand(Guid.Empty));

        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(DeleteUnitCommand.Id));
    }
}
