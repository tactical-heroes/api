using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.Policies;

public sealed class MatchmakingPairTests
{
    [Theory(DisplayName = "Pair should validate search ids when created")]
    [InlineData("Valid", true)]
    [InlineData("EmptyFirst", false)]
    [InlineData("EmptySecond", false)]
    [InlineData("Same", false)]
    public void Create_Should_ValidateSearchIds_When_Created(string scenario, bool valid)
    {
        var first = scenario == "EmptyFirst" ? Guid.Empty : Guid.CreateVersion7();
        var second = scenario == "EmptySecond" ? Guid.Empty : scenario == "Same" ? first : Guid.CreateVersion7();

        var result = MatchmakingPair.Create(first, second);

        result.IsSuccess.ShouldBe(valid);
        if (valid)
        {
            result.Value.FirstSearchId.ShouldBe(first);
            result.Value.SecondSearchId.ShouldBe(second);
        }
    }
}
