using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Delete;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Delete;

public sealed class DeleteFileCommandValidatorTests
{
    [Theory(DisplayName = "Delete file validator should validate the identifier when id validity varies")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_ValidateIdentifier_When_IdValidityVaries(bool valid)
    {
        var command = new DeleteFileCommand(valid ? Guid.NewGuid() : Guid.Empty);

        var result = new DeleteFileCommandValidator().Validate(command);

        result.IsValid.ShouldBe(valid);
        if (!valid)
        {
            result.Errors.ShouldContain(error => error.PropertyName == nameof(command.Id));
        }
    }
}
