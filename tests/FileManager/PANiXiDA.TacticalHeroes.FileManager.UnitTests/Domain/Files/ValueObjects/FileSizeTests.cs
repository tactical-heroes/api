using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.ValueObjects;

public sealed class FileSizeTests
{
    [Theory(DisplayName = "File size should preserve byte count when value is positive")]
    [InlineData(1L)]
    [InlineData(4096L)]
    [InlineData(long.MaxValue)]
    public void Create_Should_ReturnSize_When_ValueIsPositive(long value)
    {
        var result = FileSize.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Theory(DisplayName = "File size should reject nonpositive byte counts when value is not positive")]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(long.MinValue)]
    public void Create_Should_ReturnValidationFailure_When_ValueIsNotPositive(long value)
    {
        var result = FileSize.Create(value);

        result.ShouldHaveSingleError(ErrorType.Validation, "File size must be greater than zero.")
            .ShouldHaveField(nameof(FileSize));
    }

    [Fact(DisplayName = "File size should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var size = FileSize.Create(128).Value;

        var result = size.ToString();

        result.ShouldBe("FileSize { Value = 128 }");
    }
}
