using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.Update;

public sealed class UpdateFactionCommandValidatorTests
{
    [Fact(DisplayName = "Update faction validator should reject invalid values")]
    public void Validate_Should_ReturnErrors_When_ValuesAreInvalid()
    {
        var validator = new UpdateFactionCommandValidator();

        var result = validator.Validate(
            new UpdateFactionCommand(Guid.Empty, "", ""));

        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(UpdateFactionCommand.Id));
        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(UpdateFactionCommand.Name));
        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(UpdateFactionCommand.Description));
    }
}
