using PANiXiDA.TacticalHeroes.FileManager.Application.Files.UpdateContent;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.UpdateContent;

public sealed class UpdateFileContentCommandValidatorTests
{
    [Fact(DisplayName = "Content update validator should accept a readable upload when metadata is valid")]
    public void Validate_Should_AcceptReadableUpload_When_MetadataIsValid()
    {
        using var content = new MemoryStream([1]);
        var command = new UpdateFileContentCommand(Guid.NewGuid(), "text/plain", 1, content);

        var result = new UpdateFileContentCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Content update validator should report the field when upload metadata is invalid")]
    [InlineData("Id")]
    [InlineData("ContentType")]
    [InlineData("Size")]
    [InlineData("Content")]
    public void Validate_Should_ReportField_When_UploadMetadataIsInvalid(string field)
    {
        using var content = new MemoryStream([1]);
        var command = new UpdateFileContentCommand(
            field == "Id" ? Guid.Empty : Guid.NewGuid(),
            field == "ContentType" ? "text/*" : "text/plain",
            field == "Size" ? 0 : 1,
            field == "Content" ? null! : content);

        var result = new UpdateFileContentCommandValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldBe([field]);
    }
}
