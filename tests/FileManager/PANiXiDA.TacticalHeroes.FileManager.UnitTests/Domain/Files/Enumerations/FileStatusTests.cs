using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.Enumerations;

public sealed class FileStatusTests
{
    [Fact(DisplayName = "File status should preserve identifier order when called")]
    public void GetAll_Should_ReturnValuesInIdOrder_When_Called()
    {
        var values = FileStatus.GetAll();

        values.ShouldBe([FileStatus.PendingUpload, FileStatus.Ready, FileStatus.Deleting, FileStatus.Deleted]);
    }

    [Theory(DisplayName = "File status should resolve known identifiers when id is known")]
    [InlineData(1, "PendingUpload")]
    [InlineData(2, "Ready")]
    [InlineData(3, "Deleting")]
    [InlineData(4, "Deleted")]
    public void FromId_Should_ReturnValue_When_IdIsKnown(int id, string name)
    {
        var value = FileStatus.FromId(id);

        value.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "File status should reject unknown identifiers when id is unknown")]
    public void FromId_Should_Throw_When_IdIsUnknown()
    {
        Action action = () => FileStatus.FromId(0);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact(DisplayName = "File status should resolve trimmed names when name is known")]
    public void FromName_Should_ReturnValue_When_NameIsKnown()
    {
        var value = FileStatus.FromName("  PendingUpload  ");

        value.ShouldBe(FileStatus.PendingUpload);
    }

    [Fact(DisplayName = "File status should reject unknown names when name is unknown")]
    public void FromName_Should_Throw_When_NameIsUnknown()
    {
        Action action = () => FileStatus.FromName("Unknown");

        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory(DisplayName = "File status lookup should report matches when id is provided")]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportMatch_When_IdIsProvided(int id, bool expected)
    {
        var found = FileStatus.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "File status lookup should report matches when name is provided")]
    [InlineData("PendingUpload", true)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void TryFromName_Should_ReportMatch_When_NameIsProvided(string name, bool expected)
    {
        var found = FileStatus.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Fact(DisplayName = "File status should normalize known names when name is known")]
    public void Create_Should_ReturnValue_When_NameIsKnown()
    {
        var result = FileStatus.Create("  PendingUpload  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(FileStatus.PendingUpload);
    }

    [Theory(DisplayName = "File status should reject missing names when name is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string? name)
    {
        var result = FileStatus.Create(name!);

        result.ShouldHaveSingleError(ErrorType.Validation, "File status is required.")
            .ShouldHaveField(nameof(FileStatus));
    }

    [Fact(DisplayName = "File status should reject unknown names when name is unknown")]
    public void Create_Should_ReturnValidationFailure_When_NameIsUnknown()
    {
        var result = FileStatus.Create("Unknown");

        result.ShouldHaveSingleError(ErrorType.Validation, "File status 'Unknown' is invalid.")
            .ShouldHaveField(nameof(FileStatus));
    }
}
