using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.Create;

public sealed class CreateUnitCommandValidatorTests
{
    [Fact(DisplayName = "Create unit validator should reject values when values are invalid")]
    public void Validate_Should_ReturnErrors_When_ValuesAreInvalid()
    {
        var validator = new CreateUnitCommandValidator();
        var command = new CreateUnitCommand(
            Name: "",
            Description: "",
            Attack: -1,
            Defense: -1,
            Health: 0,
            MinimumDamage: -1,
            MaximumDamage: -2,
            Initiative: double.NaN,
            Speed: -1,
            Shots: 0,
            RangedAttackRange: null,
            Morale: 6,
            Luck: -1,
            FactionId: Guid.Empty);

        var result = validator.Validate(command);

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateUnitCommand.Name));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateUnitCommand.Attack));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateUnitCommand.Health));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateUnitCommand.MaximumDamage));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateUnitCommand.Initiative));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateUnitCommand.RangedAttackRange));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateUnitCommand.Morale));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateUnitCommand.Luck));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(CreateUnitCommand.FactionId));
    }
}
