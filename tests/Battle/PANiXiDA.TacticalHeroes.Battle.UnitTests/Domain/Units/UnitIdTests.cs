using PANiXiDA.TacticalHeroes.Battle.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Units;

public sealed class UnitIdTests
{
    [Fact(DisplayName = "Identifier should return its value when converted to text")]
    public void ToString_Should_ReturnValue_When_ConvertedToText()
    {
        var id = UnitId.New();

        var text = id.ToString();

        text.ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Identifier should generate UUID version seven when called")]
    public void New_Should_GenerateUuidVersionSeven_When_Called()
    {
        var id = UnitId.New();

        id.Value.Version.ShouldBe(7);
    }

    [Fact(DisplayName = "Identifier should preserve its value when input is valid")]
    public void Create_Should_PreserveValue_When_InputIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = UnitId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Identifier should reject an empty value when input is empty")]
    public void Create_Should_RejectValue_When_InputIsEmpty()
    {
        var result = UnitId.Create(Guid.Empty);

        result.IsFailure.ShouldBeTrue();
    }
}
