using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.Players;

public sealed class PlayerIdTests
{
    [Fact(DisplayName = "Player id should reject empty value when created")]
    public void Create_Should_RejectEmptyValue_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = PlayerId.Create(value);
        var empty = PlayerId.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Player id should generate distinct values when requested")]
    public void New_Should_GenerateDistinctValues_When_Requested()
    {
        var first = PlayerId.New();

        first.Value.Version.ShouldBe(7);
        first.ShouldNotBe(PlayerId.New());
    }

    [Fact(DisplayName = "Player id should display underlying value when converted to text")]
    public void ToString_Should_DisplayUnderlyingValue_When_ConvertedToText()
    {
        var id = PlayerId.New();

        id.ToString().ShouldBe(id.Value.ToString());
    }
}
