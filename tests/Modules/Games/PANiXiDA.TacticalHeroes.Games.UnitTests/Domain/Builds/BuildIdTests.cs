using PANiXiDA.TacticalHeroes.Games.Domain.Builds;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Builds;

public sealed class BuildIdTests
{
    [Fact(DisplayName = "Build id should create a version 7 identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = BuildId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Build id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = BuildId.Create(Guid.Empty);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Build id cannot be empty.");
    }

    [Fact(DisplayName = "Build id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = BuildId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
