using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Update;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Update;

public sealed class UpdateFolderCommandValidatorTests
{
    [Theory(DisplayName = "Update folder validator should accept nullable identifiers when command is valid")]
    [InlineData("Personal", true)]
    [InlineData("Avatar", false)]
    public void Validate_Should_AcceptNullableIdentifiers_When_CommandIsValid(string type, bool owned)
    {
        var command = new UpdateFolderCommand(Guid.NewGuid(), " Folder ", type, owned ? Guid.NewGuid() : null);

        var result = new UpdateFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Update folder validator should report the field when metadata is invalid")]
    [InlineData("Id")]
    [InlineData("Name")]
    [InlineData("AllowedFileType")]
    [InlineData("UserId")]
    [InlineData("ParentId")]
    public void Validate_Should_ReportField_When_MetadataIsInvalid(string field)
    {
        var command = new UpdateFolderCommand(
            field == "Id" ? Guid.Empty : Guid.NewGuid(),
            field == "Name" ? "../folder" : "Folder",
            field == "AllowedFileType" ? "unknown" : "Personal",
            field == "UserId" ? Guid.Empty : Guid.NewGuid(),
            field == "ParentId" ? Guid.Empty : null);

        var result = new UpdateFolderCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([field]);
    }
}
