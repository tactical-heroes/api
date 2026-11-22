using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.Update;

public sealed class UpdateHeroCommandValidatorTests
{
    [Fact(DisplayName = "Update hero validator should reject values when values are invalid")]
    public void Validate_Should_ReturnErrors_When_ValuesAreInvalid()
    {
        var validator = new UpdateHeroCommandValidator();
        var command = new UpdateHeroCommand(
            Id: Guid.Empty,
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
            error.PropertyName == nameof(UpdateHeroCommand.Id));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateHeroCommand.Name));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateHeroCommand.Attack));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateHeroCommand.MaximumDamage));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateHeroCommand.Initiative));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateHeroCommand.Morale));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateHeroCommand.Luck));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateHeroCommand.FactionId));
    }
}
