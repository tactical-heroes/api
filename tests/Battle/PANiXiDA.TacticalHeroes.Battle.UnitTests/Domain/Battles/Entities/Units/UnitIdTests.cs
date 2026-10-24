using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Units;

public sealed class UnitIdTests
{
    [Fact(DisplayName = "Domain value should describe its state when converted to text")]
    public void ToString_Should_DescribeState_When_ConvertedToText()
    {
        var id = UnitId.Create(Guid.Parse("11111111-2222-4333-8444-555555555555")).Value;

        var text = id.ToString();

        text.ShouldBe("11111111-2222-4333-8444-555555555555");
    }

    [Fact(DisplayName = "Unit id should contain a version seven identifier when generated")]
    public void New_Should_ReturnVersionSevenId_When_Generated()
    {
        var id = UnitId.New();

        id.Value.Version.ShouldBe(7);
        id.ShouldNotBe(UnitId.New());
    }

    [Fact(DisplayName = "Unit id should preserve its value when identifier is valid")]
    public void Create_Should_PreserveValue_When_IdentifierIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = UnitId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Unit id should reject an empty identifier when identifier is empty")]
    public void Create_Should_ReturnFailure_When_IdentifierIsEmpty()
    {
        var result = UnitId.Create(Guid.Empty);

        result.IsFailure.ShouldBeTrue();
    }
}
