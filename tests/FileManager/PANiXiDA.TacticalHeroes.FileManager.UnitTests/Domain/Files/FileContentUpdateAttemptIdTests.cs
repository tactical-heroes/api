using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files;

public sealed class FileContentUpdateAttemptIdTests
{
    [Fact(DisplayName = "File content update attempt id should create a version seven identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = FileContentUpdateAttemptId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
    }

    [Fact(DisplayName = "File content update attempt id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = FileContentUpdateAttemptId.Create(Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "File content update attempt id cannot be empty.");
    }

    [Fact(DisplayName = "File content update attempt id should preserve a valid identifier when id is valid")]
    public void Create_Should_PreserveValue_When_IdIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = FileContentUpdateAttemptId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "File content update attempt id should format its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var id = FileContentUpdateAttemptId.New();

        var result = id.ToString();

        result.ShouldBe(id.Value.ToString());
    }
}
