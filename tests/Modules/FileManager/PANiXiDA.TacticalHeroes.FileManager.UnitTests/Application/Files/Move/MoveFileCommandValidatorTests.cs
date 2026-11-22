using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Move;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Move;

public sealed class MoveFileCommandValidatorTests
{
    [Theory(DisplayName = "Move file validator should accept the command when folder is optional")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_AcceptCommand_When_FolderIsOptional(bool nested)
    {
        var command = new MoveFileCommand(Guid.NewGuid(), nested ? Guid.NewGuid() : null);

        var result = new MoveFileCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Move file validator should report the field when identifier is empty")]
    [InlineData(true, "Id")]
    [InlineData(false, "FolderId")]
    public void Validate_Should_ReportField_When_IdentifierIsEmpty(bool source, string field)
    {
        var command = new MoveFileCommand(source ? Guid.Empty : Guid.NewGuid(), source ? null : Guid.Empty);

        var result = new MoveFileCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([field]);
    }
}
