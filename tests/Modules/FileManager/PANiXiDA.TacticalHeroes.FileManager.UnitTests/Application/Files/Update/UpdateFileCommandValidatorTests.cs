using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Update;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Update;

public sealed class UpdateFileCommandValidatorTests
{
    [Theory(DisplayName = "Update file validator should accept nullable identifiers when command is valid")]
    [InlineData("Personal", true)]
    [InlineData("Avatar", false)]
    public void Validate_Should_AcceptNullableIdentifiers_When_CommandIsValid(string type, bool owned)
    {
        var command = new UpdateFileCommand(Guid.NewGuid(), " File ", type, owned ? Guid.NewGuid() : null);

        var result = new UpdateFileCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Update file validator should report the field when metadata is invalid")]
    [InlineData("Id")]
    [InlineData("Name")]
    [InlineData("Type")]
    [InlineData("UserId")]
    [InlineData("FolderId")]
    public void Validate_Should_ReportField_When_MetadataIsInvalid(string field)
    {
        var command = new UpdateFileCommand(
            field == "Id" ? Guid.Empty : Guid.NewGuid(),
            field == "Name" ? "../file" : "File",
            field == "Type" ? "unknown" : "Personal",
            field == "UserId" ? Guid.Empty : Guid.NewGuid(),
            field == "FolderId" ? Guid.Empty : null);

        var result = new UpdateFileCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([field]);
    }
}
