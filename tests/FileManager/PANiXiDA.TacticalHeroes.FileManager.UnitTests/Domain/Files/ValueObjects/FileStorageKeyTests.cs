using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.ValueObjects;

public sealed class FileStorageKeyTests
{
    [Theory(DisplayName = "File storage key should preserve its value when relative key is provided")]
    [InlineData("avatar/019f4567-89ab-7cde-8123-456789abcdef")]
    [InlineData("personal/user-id/file-id")]
    [InlineData("legacy/FILE.png")]
    public void Create_Should_PreserveValue_When_RelativeKeyIsProvided(string value)
    {
        var result = FileStorageKey.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ShouldBe(FileStorageKey.Create(value).Value);
    }

    [Theory(DisplayName = "File storage key should reject missing values when key is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_Should_ReturnValidationFailure_When_KeyIsEmpty(string? value)
    {
        var result = FileStorageKey.Create(value!);

        result.ShouldHaveSingleError(ErrorType.Validation, "File storage key cannot be empty.")
            .ShouldHaveField(nameof(FileStorageKey));
    }

    [Theory(DisplayName = "File storage key should validate UTF-8 length when key is provided")]
    [InlineData("a", 1024, true)]
    [InlineData("a", 1025, false)]
    [InlineData("я", 512, true)]
    [InlineData("я", 513, false)]
    public void Create_Should_ValidateUtf8Length_When_KeyIsProvided(string character, int length, bool valid)
    {
        var value = new string(character[0], length);

        var result = FileStorageKey.Create(value);

        result.IsSuccess.ShouldBe(valid);
    }

    [Theory(DisplayName = "File storage key should reject invalid paths when key is not canonical")]
    [InlineData("/avatar/file")]
    [InlineData("avatar/file/")]
    [InlineData("avatar//file")]
    [InlineData("avatar/./file")]
    [InlineData("avatar/../file")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("avatar\\file")]
    [InlineData(" avatar/file")]
    [InlineData("avatar/file ")]
    [InlineData("avatar/file\nname")]
    [InlineData("avatar/file\0name")]
    public void Create_Should_ReturnValidationFailure_When_KeyIsNotCanonical(string value)
    {
        var result = FileStorageKey.Create(value);

        result.ShouldHaveSingleError(ErrorType.Validation, "File storage key must be a relative path without surrounding whitespace, empty or dot segments, backslashes, or control characters.")
            .ShouldHaveField(nameof(FileStorageKey));
    }

    [Fact(DisplayName = "File storage key should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var key = FileStorageKey.Create("avatar/file-id").Value;

        var result = key.ToString();

        result.ShouldBe("FileStorageKey { Value = avatar/file-id }");
    }
}
