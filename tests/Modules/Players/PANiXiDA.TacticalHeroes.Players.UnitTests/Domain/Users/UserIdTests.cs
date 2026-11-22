using PANiXiDA.TacticalHeroes.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Players.UnitTests.Domain.Users;

public sealed class UserIdTests
{
    [Fact(DisplayName = "User id should create a version 7 identifier when invoked")]
    public void New_Should_CreateVersion7Guid_When_Invoked()
    {
        var id = UserId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "User id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = UserId.Create(Guid.Empty);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "User id cannot be empty.");
    }

    [Fact(DisplayName = "User id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = UserId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
