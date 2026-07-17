using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users;

public sealed class AccountStatusTests
{
    [Fact(DisplayName = "Create should return account status for a known name")]
    public void Create_Should_ReturnAccountStatus_When_NameIsKnown()
    {
        var result = AccountStatus.Create(" Blocked ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(AccountStatus.Blocked);
    }
}
