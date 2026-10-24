using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Units;

public sealed class UnitIdTests
{
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
        UnitId.Create(Guid.Empty).IsFailure.ShouldBeTrue();
    }
}
