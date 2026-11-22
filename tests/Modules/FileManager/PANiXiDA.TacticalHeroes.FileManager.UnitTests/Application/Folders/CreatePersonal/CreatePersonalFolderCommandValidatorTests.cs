using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.CreatePersonal;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.CreatePersonal;

public sealed class CreatePersonalFolderCommandValidatorTests
{
    [Theory(DisplayName = "Personal folder validator should accept metadata when command is valid")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_AcceptMetadata_When_CommandIsValid(bool hasParent)
    {
        var command = new CreatePersonalFolderCommand("Documents", hasParent ? Guid.NewGuid() : null);

        var result = new CreatePersonalFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Personal folder validator should report field errors when metadata is invalid")]
    [InlineData("", false, "Name")]
    [InlineData("../folder", false, "Name")]
    [InlineData("Documents", true, "ParentId")]
    public void Validate_Should_ReportFieldErrors_When_MetadataIsInvalid(string name, bool emptyParent, string field)
    {
        var command = new CreatePersonalFolderCommand(name, emptyParent ? Guid.Empty : null);

        var result = new CreatePersonalFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([field]);
    }
}
