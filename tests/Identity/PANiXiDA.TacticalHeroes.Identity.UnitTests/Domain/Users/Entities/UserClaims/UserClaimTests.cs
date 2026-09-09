using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.Entities.UserClaims;

public sealed class UserClaimTests
{
    [Fact(DisplayName = "Create should build a user claim from valid values when values are valid")]
    public void Create_Should_ReturnUserClaim_When_ValuesAreValid()
    {
        var result = UserClaim.Create(ClaimType.Create(value: " permission ").Value, ClaimValue.Create(value: " heroes.read ").Value);

        result.Id.Value.ShouldNotBe(Guid.Empty);
        result.Type.Value.ShouldBe("permission");
        result.Value.Value.ShouldBe("heroes.read");
    }

}
