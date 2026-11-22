using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.UnitTests.Domain.Files.ValueObjects;

public sealed class FileOperationAttemptIdTests
{
    [Fact(DisplayName = "File operation attempt id should create a version seven identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = FileOperationAttemptId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
    }

    [Fact(DisplayName = "File operation attempt id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = FileOperationAttemptId.Create(Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "File operation attempt id cannot be empty.");
    }

    [Fact(DisplayName = "File operation attempt id should preserve a valid identifier when id is valid")]
    public void Create_Should_PreserveValue_When_IdIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = FileOperationAttemptId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "File operation attempt id should format its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var id = FileOperationAttemptId.New();

        var result = id.ToString();

        result.ShouldBe($"FileOperationAttemptId {{ Value = {id.Value} }}");
    }
}
