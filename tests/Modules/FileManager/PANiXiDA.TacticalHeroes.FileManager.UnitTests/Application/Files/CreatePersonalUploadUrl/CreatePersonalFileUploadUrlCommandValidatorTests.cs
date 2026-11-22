using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonalUploadUrl;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.CreatePersonalUploadUrl;

public sealed class CreatePersonalFileUploadUrlCommandValidatorTests
{
    [Theory(DisplayName = "Personal upload URL validator should accept metadata when command is valid")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_AcceptMetadata_When_CommandIsValid(bool hasFolder)
    {
        var command = new CreatePersonalFileUploadUrlCommand(
            "file.txt", "text/plain", 4, hasFolder ? Guid.NewGuid() : null);

        var result = new CreatePersonalFileUploadUrlCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Personal upload URL validator should reject metadata when command is invalid")]
    public void Validate_Should_RejectMetadata_When_CommandIsInvalid()
    {
        var command = new CreatePersonalFileUploadUrlCommand("../file.txt", "text/*", 0, Guid.Empty);

        var result = new CreatePersonalFileUploadUrlCommandValidator().Validate(command);

        result.Errors.Select(error => error.PropertyName).ShouldBe(
            ["Name", "ContentType", "Size", "FolderId"], ignoreOrder: true);
    }
}
