using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.MatchmakingTickets.ValueObjects;

public sealed class MatchmakingTimestampTests
{
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
