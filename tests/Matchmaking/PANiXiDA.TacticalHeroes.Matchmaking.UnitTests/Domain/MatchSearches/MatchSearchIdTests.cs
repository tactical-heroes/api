using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches;

public sealed class MatchSearchIdTests
{
    [Fact(DisplayName = "Search id should reject empty value when created")]
    public void Create_Should_RejectEmptyValue_When_Created()
    {
        var value = Guid.CreateVersion7();
        var valid = MatchSearchId.Create(value);
        var empty = MatchSearchId.Create(Guid.Empty);

        valid.Value.Value.ShouldBe(value);
        empty.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Search id should generate distinct values when requested")]
    public void New_Should_GenerateDistinctValues_When_Requested()
    {
        var first = MatchSearchId.New();

        first.Value.Version.ShouldBe(7);
        first.ShouldNotBe(MatchSearchId.New());
    }

    [Fact(DisplayName = "Search id should display underlying value when converted to text")]
    public void ToString_Should_DisplayUnderlyingValue_When_ConvertedToText()
    {
        var id = MatchSearchId.New();

        id.ToString().ShouldBe(id.Value.ToString());
    }
}
