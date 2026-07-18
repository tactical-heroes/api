using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.Enumerations;

public sealed class UserStatusTests
{
    [Theory(DisplayName = "User status should resolve a known name")]
    [InlineData("Active", false)]
    [InlineData("Blocked", true)]
    public void Create_Should_ReturnStatus_When_NameIsKnown(
        string name,
        bool isBlocked)
    {
        var result = UserStatus.Create(name);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsBlocked.ShouldBe(isBlocked);
    }

    [Fact(DisplayName = "User status should trim a known name")]
    public void Create_Should_TrimValue_When_NameIsKnown()
    {
        var result = UserStatus.Create("  Active  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(UserStatus.Active);
    }

    [Fact(DisplayName = "User status should reject an empty value")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsEmpty()
    {
        var result = UserStatus.Create("   ");

        result.ShouldHaveSingleError(ErrorType.Validation, "User status is required.")
            .ShouldHaveField(nameof(UserStatus));
    }

    [Fact(DisplayName = "User status should reject an unknown value")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsUnknown()
    {
        var result = UserStatus.Create("Deleted");

        result.ShouldHaveSingleError(ErrorType.Validation, "User status 'Deleted' is invalid.")
            .ShouldHaveField(nameof(UserStatus));
    }
}
