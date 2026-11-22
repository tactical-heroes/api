using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles;

public sealed class BattleIdTests
{
    [Fact(DisplayName = "Domain value should describe its state when converted to text")]
    public void ToString_Should_DescribeState_When_ConvertedToText()
    {
        var id = BattleId.Create(Guid.Parse("11111111-2222-4333-8444-555555555555")).Value;

        var text = id.ToString();

        text.ShouldBe("11111111-2222-4333-8444-555555555555");
    }

    [Fact(DisplayName = "Battle id should contain a version seven identifier when generated")]
    public void New_Should_ReturnVersionSevenId_When_Generated()
    {
        var id = BattleId.New();

        id.Value.Version.ShouldBe(7);
        id.ShouldNotBe(BattleId.New());
    }

    [Fact(DisplayName = "Battle id should preserve its value when identifier is valid")]
    public void Create_Should_PreserveValue_When_IdentifierIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = BattleId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Battle id should reject an empty identifier when identifier is empty")]
    public void Create_Should_ReturnFailure_When_IdentifierIsEmpty()
    {
        var result = BattleId.Create(Guid.Empty);

        result.IsFailure.ShouldBeTrue();
    }
}
