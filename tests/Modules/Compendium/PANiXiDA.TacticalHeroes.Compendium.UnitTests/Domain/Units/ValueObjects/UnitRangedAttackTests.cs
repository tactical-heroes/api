using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units.ValueObjects;

public sealed class UnitRangedAttackTests
{
    [Theory(DisplayName = "Unit ranged attack should format nullable components when converted to string")]
    [InlineData(null, null, "UnitRangedAttack { Shots = null, RangedAttackRange = null }")]
    [InlineData(12, 8, "UnitRangedAttack { Shots = 12, RangedAttackRange = 8 }")]
    public void ToString_Should_FormatNullableComponents_When_ConvertedToString(
        int? shots,
        int? rangedAttackRange,
        string expected)
    {
        var attack = UnitRangedAttack.Create(shots, rangedAttackRange).Value;

        var result = attack.ToString();

        result.ShouldBe(expected);
    }

    [Theory(DisplayName = "Unit ranged attack should compare all nullable components when values are compared")]
    [InlineData(null, null, null, null, true)]
    [InlineData(12, 8, 12, 8, true)]
    [InlineData(12, 8, 13, 8, false)]
    [InlineData(12, 8, 12, 9, false)]
    [InlineData(null, null, 12, 8, false)]
    public void Equals_Should_CompareAllNullableComponents_When_ValuesAreCompared(
        int? shots,
        int? range,
        int? otherShots,
        int? otherRange,
        bool expected)
    {
        var attack = UnitRangedAttack.Create(shots, range).Value;
        var other = UnitRangedAttack.Create(otherShots, otherRange).Value;

        var result = attack.Equals(other);

        result.ShouldBe(expected);
    }

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
