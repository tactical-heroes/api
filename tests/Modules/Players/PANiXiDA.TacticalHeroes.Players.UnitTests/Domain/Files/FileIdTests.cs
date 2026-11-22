using PANiXiDA.TacticalHeroes.Players.Domain.Files;

namespace PANiXiDA.TacticalHeroes.Players.UnitTests.Domain.Files;

public sealed class FileIdTests
{
    [Fact(DisplayName = "File id should create a version 7 identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = FileId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "File id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = FileId.Create(Guid.Empty);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "File id cannot be empty.");
    }

    [Fact(DisplayName = "File id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = FileId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
