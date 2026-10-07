using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Folders.ValueObjects;

public sealed class FolderNameTests
{
    [Fact(DisplayName = "Folder name should trim surrounding spaces when name is valid")]
    public void Create_Should_TrimValue_When_NameIsValid()
    {
        var result = FolderName.Create("  Аватары игроков  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Аватары игроков");
    }

    [Theory(DisplayName = "Folder name should reject missing text when name is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string? value)
    {
        var result = FolderName.Create(value!);

        result.ShouldHaveSingleError(ErrorType.Validation, "Folder name cannot be empty.")
            .ShouldHaveField(nameof(FolderName));
    }

    [Theory(DisplayName = "Folder name should respect the maximum length when name is provided")]
    [InlineData(255, true)]
    [InlineData(256, false)]
    public void Create_Should_ValidateLength_When_NameIsProvided(int length, bool valid)
    {
        var value = new string('a', length);

        var result = FolderName.Create(value);

        result.IsSuccess.ShouldBe(valid);
    }

    [Theory(DisplayName = "Folder name should reject paths and control characters when name contains path or control characters")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("../Avatars")]
    [InlineData("folder/Avatars")]
    [InlineData("folder\\Avatars")]
    [InlineData("Ava\r\ntars")]
    [InlineData("Ava\0tars")]
    public void Create_Should_ReturnValidationFailure_When_NameContainsPathOrControlCharacters(string value)
    {
        var result = FolderName.Create(value);

        result.ShouldHaveSingleError(ErrorType.Validation, "Folder name cannot contain paths or control characters.")
            .ShouldHaveField(nameof(FolderName));
    }

    [Fact(DisplayName = "Folder name should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var name = FolderName.Create("Avatars").Value;

        var result = name.ToString();

        result.ShouldBe("FolderName { Value = Avatars }");
    }
}
