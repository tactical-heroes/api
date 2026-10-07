using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.ValueObjects;

public sealed class FileNameTests
{
    [Fact(DisplayName = "File name should trim surrounding spaces when name is valid")]
    public void Create_Should_TrimValue_When_NameIsValid()
    {
        var result = FileName.Create("  Лучник idle.png  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Лучник idle.png");
    }

    [Theory(DisplayName = "File name should reject missing text when name is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string? value)
    {
        var result = FileName.Create(value!);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "File name cannot be empty.")
            .ShouldHaveField(nameof(FileName));
    }

    [Theory(DisplayName = "File name should respect the maximum length when name is provided")]
    [InlineData(255, true)]
    [InlineData(256, false)]
    public void Create_Should_ValidateLength_When_NameIsProvided(
        int length,
        bool valid)
    {
        var value = new string(
            'a',
            length);

        var result = FileName.Create(value);

        result.IsSuccess.ShouldBe(valid);
    }

    [Theory(DisplayName = "File name should reject paths and control characters when name contains path or control characters")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("../avatar.png")]
    [InlineData("folder/avatar.png")]
    [InlineData("folder\\avatar.png")]
    [InlineData("avatar\r\n.png")]
    [InlineData("avatar\0.png")]
    public void Create_Should_ReturnValidationFailure_When_NameContainsPathOrControlCharacters(string value)
    {
        var result = FileName.Create(value);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "File name cannot contain paths or control characters.")
            .ShouldHaveField(nameof(FileName));
    }

    [Fact(DisplayName = "File name should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var name = FileName.Create("avatar.png").Value;

        var result = name.ToString();

        result.ShouldBe("FileName { Value = avatar.png }");
    }
}
