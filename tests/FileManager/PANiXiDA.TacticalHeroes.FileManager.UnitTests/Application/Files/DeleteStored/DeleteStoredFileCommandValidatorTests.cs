using PANiXiDA.TacticalHeroes.FileManager.Application.Files.DeleteStored;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.DeleteStored;

public sealed class DeleteStoredFileCommandValidatorTests
{
    [Theory(DisplayName = "Stored file cleanup validator should validate fields when command is provided")]
    [InlineData("contents/unused", true, true)]
    [InlineData("../active", true, false)]
    [InlineData("", true, false)]
    [InlineData("contents/unused", false, false)]
    public void Validate_Should_ValidateFields_When_CommandIsProvided(string key, bool deadline, bool valid)
    {
        var command = new DeleteStoredFileCommand(key, deadline ? DateTimeOffset.UtcNow.AddDays(1) : default);
        var validator = new DeleteStoredFileCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.ShouldBe(valid);
    }
}
