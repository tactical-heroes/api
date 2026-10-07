using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.ValueObjects;

public sealed class FileContentTypeTests
{
    [Fact(DisplayName = "Content type should normalize casing when value is valid")]
    public void Create_Should_NormalizeValue_When_ValueIsValid()
    {
        var result = FileContentType.Create(" IMAGE/PNG ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("image/png");
    }

    [Theory(DisplayName = "Content type should reject invalid media types when value is invalid")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("image")]
    [InlineData("image/")]
    [InlineData("image/*")]
    [InlineData("*/png")]
    [InlineData("image/png; charset=utf-8")]
    [InlineData("image/png\r\nX-Header: value")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsInvalid(string? value)
    {
        var result = FileContentType.Create(value!);

        result.ShouldHaveSingleError(
                ErrorType.Validation, "File content type must be a concrete media type without parameters.")
            .ShouldHaveField(nameof(FileContentType));
    }

    [Fact(DisplayName = "Content type should reject oversized values when value is too long")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsTooLong()
    {
        var value = "image/" + new string('a', FileContentType.MaxLength);

        var result = FileContentType.Create(value);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Content type should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var contentType = FileContentType.Create("image/png").Value;

        var result = contentType.ToString();

        result.ShouldBe("FileContentType { Value = image/png }");
    }
}
