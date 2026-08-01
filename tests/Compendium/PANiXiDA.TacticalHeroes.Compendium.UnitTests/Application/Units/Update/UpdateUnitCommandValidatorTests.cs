using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.Update;

public sealed class UpdateUnitCommandValidatorTests
{
    [Fact(DisplayName = "Update unit validator should reject values when values are invalid")]
    public void Validate_Should_ReturnErrors_When_ValuesAreInvalid()
    {
        var validator = new UpdateUnitCommandValidator();
        var command = new UpdateUnitCommand(
            Id: Guid.Empty,
            Name: "",
            Description: "",
            Attack: -1,
            Defense: -1,
            Health: 0,
            MinimumDamage: -1,
            MaximumDamage: -2,
            Initiative: double.NaN,
            Speed: -1,
            Shots: null,
            RangedAttackRange: 0,
            Morale: 6,
            Luck: -1,
            FactionId: Guid.Empty);

        var result = validator.Validate(command);

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateUnitCommand.Id));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateUnitCommand.Name));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateUnitCommand.Attack));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateUnitCommand.Health));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateUnitCommand.MaximumDamage));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateUnitCommand.RangedAttackRange));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateUnitCommand.Morale));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateUnitCommand.Luck));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(UpdateUnitCommand.FactionId));
    }
}
