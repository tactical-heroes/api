using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users;

public sealed class UserStatusTests
{
    [Fact(DisplayName = "Create should return user status for a known name")]
    public void Create_Should_ReturnUserStatus_When_NameIsKnown()
    {
        var result = UserStatus.Create(value: " Blocked ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(UserStatus.Blocked);
    }
}
