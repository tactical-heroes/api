using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Rename;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Rename;

public sealed class RenameFolderCommandValidatorTests
{
    [Theory(DisplayName = "Rename folder validator should accept the command when metadata is valid")]
    [InlineData("Documents")]
    [InlineData(" Documents ")]
    public void Validate_Should_AcceptCommand_When_MetadataIsValid(string name)
    {
        var command = new RenameFolderCommand(Guid.NewGuid(), name);

        var result = new RenameFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Rename folder validator should report the name when name is invalid")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("folder/child")]
    [InlineData("folder\\child")]
    [InlineData("folder\n")]
    public void Validate_Should_ReportName_When_NameIsInvalid(string? name)
    {
        var command = new RenameFolderCommand(Guid.NewGuid(), name!);

        var result = new RenameFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([nameof(command.Name)]);
    }

    [Fact(DisplayName = "Rename folder validator should report the name when name exceeds maximum length")]
    public void Validate_Should_ReportName_When_NameExceedsMaximumLength()
    {
        var command = new RenameFolderCommand(Guid.NewGuid(), new string('a', FolderName.MaxLength + 1));

        var result = new RenameFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([nameof(command.Name)]);
    }

    [Fact(DisplayName = "Rename folder validator should report the identifier when id is empty")]
    public void Validate_Should_ReportIdentifier_When_IdIsEmpty()
    {
        var command = new RenameFolderCommand(Guid.Empty, "Documents");

        var result = new RenameFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([nameof(command.Id)]);
    }
}
