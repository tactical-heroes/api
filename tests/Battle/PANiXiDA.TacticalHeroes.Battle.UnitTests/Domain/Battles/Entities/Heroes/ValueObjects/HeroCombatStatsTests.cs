using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Heroes.ValueObjects;

public sealed class HeroCombatStatsTests
{
    [Fact(DisplayName = "Domain value should describe its state when converted to text")]
    public void ToString_Should_DescribeState_When_ConvertedToText()
    {
        var value = HeroCombatStats.Create(3, 4, 2, 5, 10, 1, 1).Value;

        var text = value.ToString();

        text.ShouldBe("HeroCombatStats { Attack = 3, Defense = 4, MinimumDamage = 2, "
            + "MaximumDamage = 5, Initiative = 10, Morale = 1, Luck = 1 }");
    }

    [Theory(DisplayName = "Hero combat stats should validate damage ordering when a snapshot is created")]
    [InlineData(2, 4, true)]
    [InlineData(-1, 4, false)]
    [InlineData(4, 2, false)]
    public void Create_Should_ValidateDamageOrdering_When_ASnapshotIsCreated(int minimum, int maximum, bool valid)
    {
        var result = HeroCombatStats.Create(
            attack: 3,
            defense: 4,
            minimumDamage: minimum,
            maximumDamage: maximum,
            initiative: 10,
            morale: 1,
            luck: 1);

        result.IsSuccess.ShouldBe(valid);
    }
}
