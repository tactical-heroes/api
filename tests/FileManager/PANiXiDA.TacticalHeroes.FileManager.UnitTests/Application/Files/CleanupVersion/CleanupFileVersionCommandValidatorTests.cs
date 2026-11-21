using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CleanupVersion;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.CleanupVersion;

public sealed class CleanupFileVersionCommandValidatorTests
{
    [Theory(DisplayName = "Version cleanup validator should validate identity when command values vary")]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_Should_ValidateIdentity_When_CommandValuesVary(bool valid)
    {
        var command = new CleanupFileVersionCommand(
            valid ? Guid.NewGuid() : Guid.Empty,
            valid ? "avatar/file" : "",
            valid ? "version" : "");

        var result = new CleanupFileVersionCommandValidator().Validate(command);

        result.IsValid.ShouldBe(valid);
    }
}
