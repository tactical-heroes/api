using PANiXiDA.TacticalHeroes.Games.Domain.MatchSearches;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.MatchSearches;

public sealed class MatchSearchIdTests
{
    [Fact(DisplayName = "Match search id should create a version 7 identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = MatchSearchId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Match search id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = MatchSearchId.Create(Guid.Empty);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Match search id cannot be empty.");
    }

    [Fact(DisplayName = "Match search id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = MatchSearchId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
