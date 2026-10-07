using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.ValueObjects;

public sealed class FileChecksumTests
{
    [Fact(DisplayName = "File checksum should normalize hexadecimal casing when value is valid")]
    public void Create_Should_NormalizeValue_When_ValueIsValid()
    {
        var result = FileChecksum.Create(new string('A', 64));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(new string('a', 64));
    }

    [Theory(DisplayName = "File checksum should reject malformed values when value is invalid")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("gggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsInvalid(string? value)
    {
        var result = FileChecksum.Create(value!);

        result.ShouldHaveSingleError(
                ErrorType.Validation, "File SHA-256 must contain exactly 64 hexadecimal characters.")
            .ShouldHaveField(nameof(FileChecksum));
    }

    [Fact(DisplayName = "File checksum should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var checksum = FileChecksum.Create(new string('a', 64)).Value;

        var result = checksum.ToString();

        result.ShouldBe("FileChecksum { Value = " + new string('a', 64) + " }");
    }
}
