using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users;

public sealed class UserIdTests
{
    [Fact(DisplayName = "User id should create a non-empty value")]
    public void New_Should_CreateNonEmptyId_When_Called()
    {
        var id = UserId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "User id should preserve a valid value")]
    public void Create_Should_ReturnId_When_ValueIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = UserId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "User id should reject an empty value")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsEmpty()
    {
        var result = UserId.Create(Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "User id cannot be empty.");
    }
}
