using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Folders;

public sealed class FolderIdTests
{
    [Fact(DisplayName = "Folder id should create a version seven identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = FolderId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
    }

    [Fact(DisplayName = "Folder id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = FolderId.Create(Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "Folder id cannot be empty.");
    }

    [Fact(DisplayName = "Folder id should preserve a valid identifier when id is valid")]
    public void Create_Should_PreserveValue_When_IdIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = FolderId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Folder id should format its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var id = FolderId.New();

        var result = id.ToString();

        result.ShouldBe(id.Value.ToString());
    }
}
