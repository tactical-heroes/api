using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Units.ValueObjects;

public sealed class UnitTraitsTests
{
    [Fact(DisplayName = "Domain value should describe its state when converted to text")]
    public void ToString_Should_DescribeState_When_ConvertedToText()
    {
        var value = UnitTraits.Create(10, 5, 1, 1, 12, 5).Value;

        var text = value.ToString();

        text.ShouldBe("UnitTraits { Initiative = 10, Speed = 5, Morale = 1, Luck = 1, Shots = 12, "
            + "RangedAttackRange = 5 }");
    }

    [Theory(DisplayName = "Unit traits should validate ranged values when a snapshot is created")]
    [InlineData(12, 5, true)]
    [InlineData(null, null, true)]
    [InlineData(-1, 5, false)]
    [InlineData(12, null, false)]
    [InlineData(null, 5, false)]
    [InlineData(12, 0, false)]
    public void Create_Should_ValidateRangedValues_When_ASnapshotIsCreated(int? shots, int? range, bool valid)
    {
        var result = UnitTraits.Create(
            initiative: 10,
            speed: 5,
            morale: 1,
            luck: 1,
            shots: shots,
            rangedAttackRange: range);

        result.IsSuccess.ShouldBe(valid);
    }
}
