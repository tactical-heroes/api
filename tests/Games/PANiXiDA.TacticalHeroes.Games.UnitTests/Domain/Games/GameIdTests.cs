using PANiXiDA.TacticalHeroes.Games.Domain.Games;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Games;

public sealed class GameIdTests
{
    [Fact(DisplayName = "Game id should create a version 7 identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = GameId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Game id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = GameId.Create(Guid.Empty);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Game id cannot be empty.");
    }

    [Fact(DisplayName = "Game id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = GameId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
