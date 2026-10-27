using PANiXiDA.TacticalHeroes.Builds.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Builds.UnitTests.Domain.Players;

public sealed class PlayerIdTests
{
    [Fact(DisplayName = "Player id should create a version 7 identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = PlayerId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Player id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = PlayerId.Create(Guid.Empty);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Player id cannot be empty.");
    }

    [Fact(DisplayName = "Player id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = PlayerId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
