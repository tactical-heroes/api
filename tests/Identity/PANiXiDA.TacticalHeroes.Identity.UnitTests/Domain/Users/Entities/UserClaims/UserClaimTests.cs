using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.Entities.UserClaims;

public sealed class UserClaimTests
{
    [Fact(DisplayName = "Create should build a user claim from valid values")]
    public void Create_Should_ReturnUserClaim_When_ValuesAreValid()
    {
        var result = UserClaim.Create(" permission ", " heroes.read ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.Value.ShouldNotBe(Guid.Empty);
        result.Value.Type.Value.ShouldBe("permission");
        result.Value.Value.Value.ShouldBe("heroes.read");
    }

    [Fact(DisplayName = "Create should reject an invalid user claim value")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsInvalid()
    {
        var result = UserClaim.Create("permission", "");

        result.ShouldHaveSingleError(ErrorType.Validation, "Claim value cannot be empty.");
    }
}
