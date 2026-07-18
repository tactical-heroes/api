using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.Entities.UserClaims;

public sealed class UserClaimIdTests
{
    [Fact(DisplayName = "User claim id should create a non-empty value")]
    public void New_Should_CreateNonEmptyId_When_Called()
    {
        var id = UserClaimId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "User claim id should preserve a valid value")]
    public void Create_Should_ReturnId_When_ValueIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = UserClaimId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "User claim id should reject an empty value")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsEmpty()
    {
        var result = UserClaimId.Create(Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "User claim id cannot be empty.");
    }
}
