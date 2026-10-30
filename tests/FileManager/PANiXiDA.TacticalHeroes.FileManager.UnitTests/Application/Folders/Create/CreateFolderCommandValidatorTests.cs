using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Create;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Create;

public sealed class CreateFolderCommandValidatorTests
{
    [Theory(DisplayName = "Create folder validator should accept optional identifiers when command is valid")]
    [InlineData("Personal", false)]
    [InlineData("Personal", true)]
    [InlineData("Avatar", false)]
    [InlineData("Avatar", true)]
    public void Validate_Should_AcceptOptionalIdentifiers_When_CommandIsValid(string type, bool hasIdentifiers)
    {
        var command = new CreateFolderCommand(
            "Documents", type, hasIdentifiers ? Guid.NewGuid() : null, hasIdentifiers ? Guid.NewGuid() : null);

        var result = new CreateFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Create folder validator should report field errors when metadata is invalid")]
    [InlineData("", "Personal", "Name")]
    [InlineData("../folder", "Personal", "Name")]
    [InlineData("Documents", "", "AllowedFileType")]
    [InlineData("Documents", "Other", "AllowedFileType")]
    [InlineData("Documents", "Personal", "UserId")]
    [InlineData("Documents", "Personal", "ParentId")]
    public void Validate_Should_ReportFieldErrors_When_MetadataIsInvalid(string name, string type, string field)
    {
        var command = new CreateFolderCommand(
            name, type, field == "UserId" ? Guid.Empty : null, field == "ParentId" ? Guid.Empty : null);

        var result = new CreateFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([field]);
    }
}
