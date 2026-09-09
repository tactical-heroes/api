using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units.ValueObjects;

public sealed class UnitRangedAttackTests
{
    [Theory(DisplayName = "Unit ranged attack should preserve values when values are valid")]
    [InlineData(null, null)]
    [InlineData(1, 1)]
    [InlineData(12, 8)]
    public void Create_Should_ReturnRangedAttack_When_ValuesAreValid(
        int? shots,
        int? rangedAttackRange)
    {
        var result = UnitRangedAttack.Create(shots, rangedAttackRange);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Shots.ShouldBe(shots);
        result.Value.RangedAttackRange.ShouldBe(rangedAttackRange);
    }

    [Theory(DisplayName = "Unit ranged attack should reject incomplete or nonpositive values when values are invalid")]
    [InlineData(12, null, nameof(UnitRangedAttack.RangedAttackRange))]
    [InlineData(null, 8, nameof(UnitRangedAttack.RangedAttackRange))]
    [InlineData(0, 8, nameof(UnitRangedAttack.Shots))]
    [InlineData(-1, 8, nameof(UnitRangedAttack.Shots))]
    [InlineData(12, 0, nameof(UnitRangedAttack.RangedAttackRange))]
    [InlineData(12, -1, nameof(UnitRangedAttack.RangedAttackRange))]
    public void Create_Should_ReturnValidationFailure_When_ValuesAreInvalid(
        int? shots,
        int? rangedAttackRange,
        string field)
    {
        var result = UnitRangedAttack.Create(shots, rangedAttackRange);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        result.Errors.ShouldContain(error =>
            Equals(error.Metadata.GetValueOrDefault(Error.FieldMetadataKey), field));
    }
}
