using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Users;

public sealed class UserIdTests
{
    [Fact(DisplayName = "User id should reject an empty identifier when id is empty")]
    public void Create_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var result = UserId.Create(Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "User id cannot be empty.");
    }

    [Fact(DisplayName = "User id should preserve an external identifier when id is valid")]
    public void Create_Should_PreserveValue_When_IdIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = UserId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Optional user id should return null when id is omitted")]
    public void Create_Should_ReturnNull_When_IdIsOmitted()
    {
        var result = UserId.Create(value: null);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact(DisplayName = "Optional user id should reject an empty identifier when nullable id is empty")]
    public void Create_Should_ReturnValidationFailure_When_NullableIdIsEmpty()
    {
        var result = UserId.Create((Guid?)Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "User id cannot be empty.");
    }

    [Fact(DisplayName = "Optional user id should preserve an external identifier when nullable id is valid")]
    public void Create_Should_PreserveValue_When_NullableIdIsValid()
    {
        var value = Guid.NewGuid();

        var result = UserId.Create((Guid?)value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "User id should format its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var id = UserId.Create(Guid.CreateVersion7()).Value;

        var result = id.ToString();

        result.ShouldBe(id.Value.ToString());
    }
}
