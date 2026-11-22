using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.UnitTests.Domain.Files.ValueObjects;

public sealed class FileVersionIdTests
{
    [Theory(DisplayName = "File version should preserve an opaque identifier when storage version is valid")]
    [InlineData("opaque/version+id==")]
    [InlineData("Null")]
    [InlineData(" version ")]
    public void Create_Should_PreserveValue_When_StorageVersionIsValid(string value)
    {
        var result = FileVersionId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Theory(DisplayName = "File version should reject identifiers when storage version is unavailable")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("null")]
    public void Create_Should_RejectValue_When_StorageVersionIsUnavailable(string? value)
    {
        var result = FileVersionId.Create(value);

        result.IsFailure.ShouldBeTrue();
    }

    [Theory(DisplayName = "File version should enforce length when identifier is provided")]
    [InlineData(FileVersionId.MaxLength, true)]
    [InlineData(FileVersionId.MaxLength + 1, false)]
    public void Create_Should_EnforceLength_When_IdentifierIsProvided(int length, bool valid)
    {
        var result = FileVersionId.Create(new string('v', length));

        result.IsSuccess.ShouldBe(valid);
    }

    [Fact(DisplayName = "File version should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var version = FileVersionId.Create("opaque/version+id==").Value;

        var result = version.ToString();

        result.ShouldBe("FileVersionId { Value = opaque/version+id== }");
    }
}
