using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Delete;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Delete;

public sealed class DeleteFolderCommandValidatorTests
{
    [Theory(DisplayName = "Delete folder validator should validate the identifier when id validity varies")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_ValidateIdentifier_When_IdValidityVaries(bool valid)
    {
        var command = new DeleteFolderCommand(valid ? Guid.NewGuid() : Guid.Empty);

        var result = new DeleteFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBe(valid);
        if (!valid)
        {
            result.Errors.Select(error => error.PropertyName).ShouldBe([nameof(command.Id)]);
        }
    }
}
