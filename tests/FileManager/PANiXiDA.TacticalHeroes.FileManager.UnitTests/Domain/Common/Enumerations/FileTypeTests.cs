using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Common.Enumerations;

public sealed class FileTypeTests
{
    [Fact(DisplayName = "File type should preserve identifier order when called")]
    public void GetAll_Should_ReturnValuesInIdOrder_When_Called()
    {
        var values = FileType.GetAll();

        values.ShouldBe([FileType.Personal, FileType.Avatar]);
    }

    [Theory(DisplayName = "File type should resolve known identifiers when id is known")]
    [InlineData(1, "Personal")]
    [InlineData(2, "Avatar")]
    public void FromId_Should_ReturnValue_When_IdIsKnown(int id, string name)
    {
        var value = FileType.FromId(id);

        value.Name.ShouldBe(name);
    }

    [Fact(DisplayName = "File type should reject unknown identifiers when id is unknown")]
    public void FromId_Should_Throw_When_IdIsUnknown()
    {
        Action action = () => FileType.FromId(0);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact(DisplayName = "File type should resolve trimmed names when name is known")]
    public void FromName_Should_ReturnValue_When_NameIsKnown()
    {
        var value = FileType.FromName("  Avatar  ");

        value.ShouldBe(FileType.Avatar);
    }

    [Fact(DisplayName = "File type should reject unknown names when name is unknown")]
    public void FromName_Should_Throw_When_NameIsUnknown()
    {
        Action action = () => FileType.FromName("Unknown");

        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory(DisplayName = "File type lookup should report matches when id is provided")]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportMatch_When_IdIsProvided(int id, bool expected)
    {
        var found = FileType.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "File type lookup should report matches when name is provided")]
    [InlineData("Avatar", true)]
    [InlineData("Personal", true)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void TryFromName_Should_ReportMatch_When_NameIsProvided(string name, bool expected)
    {
        var found = FileType.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "File type should normalize known names when name is known")]
    [InlineData("Avatar")]
    [InlineData("Personal")]
    public void Create_Should_ReturnValue_When_NameIsKnown(string name)
    {
        var result = FileType.Create($"  {name}  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(name);
    }

    [Theory(DisplayName = "File type should reject missing names when name is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string? name)
    {
        var result = FileType.Create(name!);

        result.ShouldHaveSingleError(ErrorType.Validation, "File type is required.")
            .ShouldHaveField(nameof(FileType));
    }

    [Fact(DisplayName = "File type should reject unknown names when name is unknown")]
    public void Create_Should_ReturnValidationFailure_When_NameIsUnknown()
    {
        var result = FileType.Create("Unknown");

        result.ShouldHaveSingleError(ErrorType.Validation, "File type 'Unknown' is invalid.")
            .ShouldHaveField(nameof(FileType));
    }
}
