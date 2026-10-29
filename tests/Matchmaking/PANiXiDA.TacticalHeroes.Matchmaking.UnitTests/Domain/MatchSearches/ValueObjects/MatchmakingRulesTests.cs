using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.ValueObjects;

public sealed class MatchmakingRulesTests
{
    [Theory(DisplayName = "Rules should validate ranges when created")]
    [InlineData(100, 50, 15, 500, true)]
    [InlineData(0, 1, 1, 0, true)]
    [InlineData(-1, 50, 15, 500, false)]
    [InlineData(100, 0, 15, 500, false)]
    [InlineData(100, 50, 0, 500, false)]
    [InlineData(100, 50, 15, 99, false)]
    public void Create_Should_ValidateRanges_When_Created(int initial, int step, int seconds, int maximum, bool valid)
    {
        var interval = TimeSpan.FromSeconds(seconds);

        var result = MatchmakingRules.Create(initial, step, interval, maximum);

        result.IsSuccess.ShouldBe(valid);
        if (valid)
        {
            result.Value.InitialRatingGap.ShouldBe(initial);
            result.Value.RatingGapStep.ShouldBe(step);
            result.Value.ExpansionInterval.ShouldBe(interval);
            result.Value.MaximumRatingGap.ShouldBe(maximum);
        }
    }

    [Fact(DisplayName = "Rules should expose state when converted to text")]
    public void ToString_Should_ExposeState_When_ConvertedToText()
    {
        var rules = MatchmakingRules.Create(100, 50, TimeSpan.FromSeconds(15), 500).Value;

        var text = rules.ToString();

        text.ShouldNotBeNullOrWhiteSpace();
    }
}
