using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CompleteUpload;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.CompleteUpload;

public sealed class CompleteFileUploadCommandValidatorTests
{
    [Theory(DisplayName = "Complete upload validator should validate the identifier when command is supplied")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_ValidateIdentifier_When_CommandIsSupplied(bool emptyId)
    {
        var command = new CompleteFileUploadCommand(emptyId ? Guid.Empty : Guid.NewGuid());

        var result = new CompleteFileUploadCommandValidator().Validate(command);

        result.IsValid.ShouldBe(!emptyId);
        if (emptyId)
        {
            result.Errors.ShouldContain(error => error.PropertyName == nameof(command.Id));
        }
    }
}
