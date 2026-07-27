using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Delete;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.Delete;

public sealed class DeleteFactionCommandValidatorTests
{
    [Fact(DisplayName = "Delete faction validator should reject an empty identifier")]
    public void Validate_Should_ReturnError_When_IdIsEmpty()
    {
        var validator = new DeleteFactionCommandValidator();

        var result = validator.Validate(new DeleteFactionCommand(Guid.Empty));

        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(DeleteFactionCommand.Id));
    }
}
