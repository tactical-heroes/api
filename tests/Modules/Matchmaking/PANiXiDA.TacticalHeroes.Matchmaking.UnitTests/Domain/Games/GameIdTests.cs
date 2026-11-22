using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.Games;

public sealed class GameIdTests
{
    [Fact(DisplayName = "Game id should reject empty value when created")]
    public void Create_Should_RejectEmptyValue_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = GameId.Create(value);
        var empty = GameId.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Game id should generate distinct values when requested")]
    public void New_Should_GenerateDistinctValues_When_Requested()
    {
        var first = GameId.New();

        first.Value.Version.ShouldBe(7);
        first.ShouldNotBe(GameId.New());
    }

    [Fact(DisplayName = "Game id should display underlying value when converted to text")]
    public void ToString_Should_DisplayUnderlyingValue_When_ConvertedToText()
    {
        var id = GameId.New();

        id.ToString().ShouldBe(id.Value.ToString());
    }
}
