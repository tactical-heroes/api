using PANiXiDA.TacticalHeroes.Battle.Domain.Games;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Games;

public sealed class GameIdTests
{
    [Fact(DisplayName = "Identifier should return its value when converted to text")]
    public void ToString_Should_ReturnValue_When_ConvertedToText()
    {
        var id = GameId.New();

        var text = id.ToString();

        text.ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Identifier should generate UUID version seven when called")]
    public void New_Should_GenerateUuidVersionSeven_When_Called()
    {
        var id = GameId.New();

        id.Value.Version.ShouldBe(7);
    }

    [Fact(DisplayName = "Identifier should preserve its value when input is valid")]
    public void Create_Should_PreserveValue_When_InputIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = GameId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Identifier should reject an empty value when input is empty")]
    public void Create_Should_RejectValue_When_InputIsEmpty()
    {
        var result = GameId.Create(Guid.Empty);

        result.IsFailure.ShouldBeTrue();
    }
}
