using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.Enumerations;

public sealed class FilePurposeTests
{
    [Fact(DisplayName = "File purpose should preserve identifier order when called")]
    public void GetAll_Should_ReturnValuesInIdOrder_When_Called()
    {
        var values = FilePurpose.GetAll();

        values.ShouldBe([FilePurpose.Avatar]);
    }

    [Fact(DisplayName = "File purpose should resolve known identifiers when id is known")]
    public void FromId_Should_ReturnValue_When_IdIsKnown()
    {
        var value = FilePurpose.FromId(1);

        value.ShouldBe(FilePurpose.Avatar);
    }

    [Fact(DisplayName = "File purpose should reject unknown identifiers when id is unknown")]
    public void FromId_Should_Throw_When_IdIsUnknown()
    {
        Action action = () => FilePurpose.FromId(0);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact(DisplayName = "File purpose should resolve trimmed names when name is known")]
    public void FromName_Should_ReturnValue_When_NameIsKnown()
    {
        var value = FilePurpose.FromName("  Avatar  ");

        value.ShouldBe(FilePurpose.Avatar);
    }

    [Fact(DisplayName = "File purpose should reject unknown names when name is unknown")]
    public void FromName_Should_Throw_When_NameIsUnknown()
    {
        Action action = () => FilePurpose.FromName("Unknown");

        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory(DisplayName = "File purpose lookup should report matches when id is provided")]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void TryFromId_Should_ReportMatch_When_IdIsProvided(int id, bool expected)
    {
        var found = FilePurpose.TryFromId(id, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Theory(DisplayName = "File purpose lookup should report matches when name is provided")]
    [InlineData("Avatar", true)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void TryFromName_Should_ReportMatch_When_NameIsProvided(string name, bool expected)
    {
        var found = FilePurpose.TryFromName(name, out var value);

        found.ShouldBe(expected);
        (value is not null).ShouldBe(expected);
    }

    [Fact(DisplayName = "File purpose should normalize known names when name is known")]
    public void Create_Should_ReturnValue_When_NameIsKnown()
    {
        var result = FilePurpose.Create("  Avatar  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(FilePurpose.Avatar);
    }

    [Theory(DisplayName = "File purpose should reject missing names when name is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string? name)
    {
        var result = FilePurpose.Create(name!);

        result.ShouldHaveSingleError(ErrorType.Validation, "File purpose is required.")
            .ShouldHaveField(nameof(FilePurpose));
    }

    [Fact(DisplayName = "File purpose should reject unknown names when name is unknown")]
    public void Create_Should_ReturnValidationFailure_When_NameIsUnknown()
    {
        var result = FilePurpose.Create("Unknown");

        result.ShouldHaveSingleError(ErrorType.Validation, "File purpose 'Unknown' is invalid.")
            .ShouldHaveField(nameof(FilePurpose));
    }
}
