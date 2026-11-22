using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Create;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Create;

public sealed class CreateFileCommandValidatorTests
{
    [Theory(DisplayName = "Create file validator should accept metadata when command is valid")]
    [InlineData("Personal", true)]
    [InlineData("Avatar", false)]
    public void Validate_Should_AcceptMetadata_When_CommandIsValid(string type, bool hasIdentifiers)
    {
        var command = new CreateFileCommand(
            Name: "file.txt",
            Type: type,
            ContentType: "text/plain",
            Size: 4,
            Content: Stream.Null,
            UserId: hasIdentifiers ? Guid.NewGuid() : null,
            FolderId: hasIdentifiers ? Guid.NewGuid() : null);

        var result = new CreateFileCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Create file validator should reject metadata when command is invalid")]
    public void Validate_Should_RejectMetadata_When_CommandIsInvalid()
    {
        var command = new CreateFileCommand("../file.txt", "Unknown", "text/*", 0, null!, Guid.Empty, Guid.Empty);

        var result = new CreateFileCommandValidator().Validate(command);

        result.Errors.Select(error => error.PropertyName).ShouldBe(
            ["Name", "Type", "ContentType", "Size", "UserId", "FolderId", "Content"], ignoreOrder: true);
    }
}
