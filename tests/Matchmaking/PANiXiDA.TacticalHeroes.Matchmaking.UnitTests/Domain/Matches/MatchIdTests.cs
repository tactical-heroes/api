using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.Matches;

public sealed class MatchIdTests
{
    [Fact(DisplayName = "Match id should reject empty value when created")]
    public void Create_Should_RejectEmptyValue_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = MatchId.Create(value);
        var empty = MatchId.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Match id should generate distinct values when requested")]
    public void New_Should_GenerateDistinctValues_When_Requested()
    {
        var first = MatchId.New();

        first.Value.Version.ShouldBe(7);
        first.ShouldNotBe(MatchId.New());
    }

    [Fact(DisplayName = "Match id should display underlying value when converted to text")]
    public void ToString_Should_DisplayUnderlyingValue_When_ConvertedToText()
    {
        var id = MatchId.New();

        id.ToString().ShouldBe(id.Value.ToString());
    }
}
