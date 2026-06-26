using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Specifications;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users;

public sealed class UserSpecificationsTests
{
    [Fact(DisplayName = "User by email specification should match normalized email")]
    public void UserByEmailSpecification_Should_MatchNormalizedEmail()
    {
        var user = CreateUser("hero@example.com");
        var specification = new UserByEmailSpecification(" HERO@EXAMPLE.COM ");

        specification.IsSatisfiedBy(user).ShouldBeTrue();
    }

    [Fact(DisplayName = "User by email specification should not match invalid email")]
    public void UserByEmailSpecification_Should_NotMatchInvalidEmail()
    {
        var user = CreateUser("hero@example.com");
        var specification = new UserByEmailSpecification("not-an-email");

        specification.IsSatisfiedBy(user).ShouldBeFalse();
    }

    private static User CreateUser(string email)
    {
        return User.Register(
                email,
                "password-hash",
                "confirmation-token-hash",
                DateTimeOffset.UtcNow.AddHours(1),
                "confirmation-token")
            .Value;
    }
}
