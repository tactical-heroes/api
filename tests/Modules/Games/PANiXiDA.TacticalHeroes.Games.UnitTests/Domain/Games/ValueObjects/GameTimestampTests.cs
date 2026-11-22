using PANiXiDA.TacticalHeroes.Games.Domain.Games.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Games.ValueObjects;

public sealed class GameTimestampTests
{
    [Fact(DisplayName = "Game timestamp should normalize time when offset is provided")]
    public void Create_Should_NormalizeTime_When_OffsetIsProvided()
    {
        var value = new DateTimeOffset(2026, 10, 27, 16, 0, 0, TimeSpan.FromHours(4));

        var timestamp = GameTimestamp.Create(value);

        timestamp.Value.ShouldBe(value.ToUniversalTime());
        timestamp.Value.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Fact(DisplayName = "Game timestamp should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var value = DateTimeOffset.UnixEpoch;
        var timestamp = GameTimestamp.Create(value);

        var formatted = timestamp.ToString();

        formatted.ShouldBe($"GameTimestamp {{ Value = {value} }}");
    }
}
