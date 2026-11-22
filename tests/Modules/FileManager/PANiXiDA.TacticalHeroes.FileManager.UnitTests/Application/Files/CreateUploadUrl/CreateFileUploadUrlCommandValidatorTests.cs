using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreateUploadUrl;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.CreateUploadUrl;

public sealed class CreateFileUploadUrlCommandValidatorTests
{
    [Theory(DisplayName = "Upload URL validator should accept metadata when optional identifiers are valid")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_AcceptMetadata_When_OptionalIdentifiersAreValid(bool hasIdentifiers)
    {
        var command = new CreateFileUploadUrlCommand(
            Name: "file.txt",
            Type: "Personal",
            ContentType: "text/plain",
            Size: 4,
            UserId: hasIdentifiers ? Guid.NewGuid() : null,
            FolderId: hasIdentifiers ? Guid.NewGuid() : null);

        var result = new CreateFileUploadUrlCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Upload URL validator should reject metadata when command is invalid")]
    public void Validate_Should_RejectMetadata_When_CommandIsInvalid()
    {
        var command = new CreateFileUploadUrlCommand("../file.txt", "Unknown", "text/*", 0, Guid.Empty, Guid.Empty);

        var result = new CreateFileUploadUrlCommandValidator().Validate(command);

        result.Errors.Select(error => error.PropertyName).ShouldBe(
            ["Name", "Type", "ContentType", "Size", "UserId", "FolderId"], ignoreOrder: true);
    }
}
