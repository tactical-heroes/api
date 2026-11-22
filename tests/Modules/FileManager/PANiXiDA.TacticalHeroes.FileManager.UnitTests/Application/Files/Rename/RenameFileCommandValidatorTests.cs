using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Rename;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Rename;

public sealed class RenameFileCommandValidatorTests
{
    [Theory(DisplayName = "Rename file validator should accept the command when metadata is valid")]
    [InlineData("file.txt")]
    [InlineData(" file.txt ")]
    public void Validate_Should_AcceptCommand_When_MetadataIsValid(string name)
    {
        var command = new RenameFileCommand(Guid.NewGuid(), name);

        var result = new RenameFileCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Rename file validator should report the name when name is invalid")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("file/child")]
    [InlineData("file\\child")]
    [InlineData("file\n")]
    public void Validate_Should_ReportName_When_NameIsInvalid(string? name)
    {
        var command = new RenameFileCommand(Guid.NewGuid(), name!);

        var result = new RenameFileCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([nameof(command.Name)]);
    }

    [Fact(DisplayName = "Rename file validator should report the name when name exceeds maximum length")]
    public void Validate_Should_ReportName_When_NameExceedsMaximumLength()
    {
        var command = new RenameFileCommand(Guid.NewGuid(), new string('a', FileName.MaxLength + 1));

        var result = new RenameFileCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([nameof(command.Name)]);
    }

    [Fact(DisplayName = "Rename file validator should report the identifier when id is empty")]
    public void Validate_Should_ReportIdentifier_When_IdIsEmpty()
    {
        var command = new RenameFileCommand(Guid.Empty, "file.txt");

        var result = new RenameFileCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([nameof(command.Id)]);
    }
}
