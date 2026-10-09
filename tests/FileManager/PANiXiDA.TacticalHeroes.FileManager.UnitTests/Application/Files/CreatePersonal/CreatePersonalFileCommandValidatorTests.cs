using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonal;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.CreatePersonal;

public sealed class CreatePersonalFileCommandValidatorTests
{
    [Fact(DisplayName = "Create personal file validator should reject invalid metadata when command is invalid")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new CreatePersonalFileCommandValidator();
        var command = new CreatePersonalFileCommand(
            "../file.txt", "text/*", 0, Stream.Null, Guid.Empty, Guid.Empty);

        var result = validator.Validate(command);

        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(command.Name));
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(command.ContentType));
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(command.Size));
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(command.UserId));
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(command.FolderId));
    }

    [Theory(DisplayName = "Create personal file validator should accept an optional folder when command is valid")]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_Should_Succeed_When_CommandIsValid(bool hasFolder)
    {
        using var content = new MemoryStream([1]);
        var command = new CreatePersonalFileCommand(
            "file.txt", "text/plain", content.Length, content, Guid.NewGuid(),
            hasFolder ? Guid.NewGuid() : null);

        var result = new CreatePersonalFileCommandValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Create personal file validator should reject missing content when stream is missing")]
    public void Validate_Should_ReturnContentError_When_StreamIsMissing()
    {
        var command = new CreatePersonalFileCommand(
            "file.txt", "text/plain", 1, null!, Guid.NewGuid(), FolderId: null);

        var result = new CreatePersonalFileCommandValidator().Validate(command);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(command.Content));
    }
}
