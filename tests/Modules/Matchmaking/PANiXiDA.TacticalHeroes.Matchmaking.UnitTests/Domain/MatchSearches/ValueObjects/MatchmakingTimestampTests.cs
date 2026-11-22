using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchSearches.ValueObjects;

public sealed class MatchmakingTimestampTests
{
    [Theory(DisplayName = "Timestamp should compare instants when values have different offsets")]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Compare_Should_CompareInstants_When_ValuesHaveDifferentOffsets(int seconds)
    {
        var instant = new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.FromHours(4));
        var left = MatchmakingTimestamp.Create(instant.AddSeconds(seconds));
        var right = MatchmakingTimestamp.Create(instant.ToUniversalTime());

        var less = left < right;
        var greater = left > right;
        var lessOrEqual = left <= right;
        var greaterOrEqual = left >= right;

        less.ShouldBe(seconds < 0);
        greater.ShouldBe(seconds > 0);
        lessOrEqual.ShouldBe(seconds <= 0);
        greaterOrEqual.ShouldBe(seconds >= 0);
    }

    [Fact(DisplayName = "Timestamp should normalize to utc when input has offset")]
    public void Create_Should_NormalizeToUtc_When_InputHasOffset()
    {
        var instant = new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.FromHours(4));
        var timestamp = MatchmakingTimestamp.Create(instant);

        timestamp.Value.ShouldBe(instant);
        timestamp.Value.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Fact(DisplayName = "Timestamp should expose state when converted to text")]
    public void ToString_Should_ExposeState_When_ConvertedToText()
    {
        var timestamp = MatchmakingTimestamp.Create(DateTimeOffset.UnixEpoch);

        timestamp.ToString().ShouldNotBeNullOrWhiteSpace();
    }
}
