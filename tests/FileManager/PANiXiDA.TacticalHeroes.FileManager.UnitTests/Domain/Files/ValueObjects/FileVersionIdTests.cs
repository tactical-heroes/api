using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.ValueObjects;

public sealed class FileVersionIdTests
{
    [Theory(DisplayName = "File version should preserve an opaque storage identifier")]
    [InlineData("opaque/version+id==")]
    [InlineData("Null")]
    [InlineData(" version ")]
    public void Create_Should_PreserveValue_When_StorageVersionIsValid(string value)
    {
        var result = FileVersionId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Theory(DisplayName = "File version should reject absent or unversioned identifiers")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("null")]
    public void Create_Should_RejectValue_When_StorageVersionIsUnavailable(string? value)
    {
        var result = FileVersionId.Create(value);

        result.IsFailure.ShouldBeTrue();
    }

    [Theory(DisplayName = "File version should enforce the maximum identifier length")]
    [InlineData(FileVersionId.MaxLength, true)]
    [InlineData(FileVersionId.MaxLength + 1, false)]
    public void Create_Should_EnforceLength_When_IdentifierIsProvided(int length, bool valid)
    {
        var result = FileVersionId.Create(new string('v', length));

        result.IsSuccess.ShouldBe(valid);
    }
}
