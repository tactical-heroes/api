using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.Create;

public sealed class CreateHeroCommandValidatorTests
{
    [Fact(DisplayName = "Create hero validator should reject values when values are invalid")]
    public void Validate_Should_ReturnErrors_When_ValuesAreInvalid()
    {
        var validator = new CreateHeroCommandValidator();
        var command = new CreateHeroCommand(
            Name: "",
            Description: "",
            Attack: -1,
            Defense: -1,
            MinimumDamage: -1,
            MaximumDamage: -2,
            Initiative: double.NaN,
            Morale: 6,
            Luck: -1,
            FactionId: Guid.Empty);

        var result = validator.Validate(command);

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateHeroCommand.Name));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateHeroCommand.Attack));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateHeroCommand.MaximumDamage));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateHeroCommand.Initiative));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateHeroCommand.Morale));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateHeroCommand.Luck));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateHeroCommand.FactionId));
    }
}
