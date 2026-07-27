using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.Create;

public sealed class CreateFactionCommandValidatorTests
{
    [Fact(DisplayName = "Create faction validator should reject empty details")]
    public void Validate_Should_ReturnErrors_When_DetailsAreEmpty()
    {
        var validator = new CreateFactionCommandValidator();

        var result = validator.Validate(new CreateFactionCommand("", ""));

        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(CreateFactionCommand.Name));
        result.Errors.ShouldContain(
            error => error.PropertyName == nameof(CreateFactionCommand.Description));
    }
}
