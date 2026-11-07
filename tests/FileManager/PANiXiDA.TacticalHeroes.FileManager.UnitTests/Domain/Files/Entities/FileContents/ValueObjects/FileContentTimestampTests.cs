using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.Entities.FileContents.ValueObjects;

public sealed class FileContentTimestampTests
{
    [Fact(DisplayName = "Content timestamp should normalize the instant when an offset is provided")]
    public void Create_Should_NormalizeInstant_When_AnOffsetIsProvided()
    {
        var value = new DateTimeOffset(2026, 11, 7, 18, 0, 0, TimeSpan.FromHours(3));

        var timestamp = FileContentTimestamp.Create(value);

        timestamp.Value.ShouldBe(value.ToUniversalTime());
        timestamp.Value.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Fact(DisplayName = "Content timestamp should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var timestamp = FileContentTimestamp.Create(DateTimeOffset.UtcNow);

        var value = timestamp.ToString();

        value.ShouldBe($"FileContentTimestamp {{ Value = {timestamp.Value} }}");
    }
}
