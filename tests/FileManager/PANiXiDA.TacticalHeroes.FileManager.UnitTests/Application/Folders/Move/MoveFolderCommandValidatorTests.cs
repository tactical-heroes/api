using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Move;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Move;

public sealed class MoveFolderCommandValidatorTests
{
    [Theory(DisplayName = "Move folder validator should accept the command when parent is optional")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_AcceptCommand_When_ParentIsOptional(bool nested)
    {
        var command = new MoveFolderCommand(Guid.NewGuid(), nested ? Guid.NewGuid() : null);

        var result = new MoveFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Move folder validator should report the field when identifier is empty")]
    [InlineData(true, "Id")]
    [InlineData(false, "ParentId")]
    public void Validate_Should_ReportField_When_IdentifierIsEmpty(bool source, string field)
    {
        var command = new MoveFolderCommand(source ? Guid.Empty : Guid.NewGuid(), source ? null : Guid.Empty);

        var result = new MoveFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([field]);
    }
}
