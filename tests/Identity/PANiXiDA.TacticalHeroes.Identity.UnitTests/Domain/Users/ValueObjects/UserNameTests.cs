using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.ValueObjects;

public sealed class UserNameTests
{
    [Fact(DisplayName = "User name should trim a valid value when user name is valid")]
    public void Create_Should_TrimValue_When_UserNameIsValid()
    {
        var result = UserName.Create("  tactical-hero  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("tactical-hero");
        result.Value.ToString().ShouldBe("tactical-hero");
    }

    [Theory(DisplayName = "User name should reject an empty value when user name is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_UserNameIsEmpty(string value)
    {
        var result = UserName.Create(value);

        result.ShouldHaveSingleError(ErrorType.Validation, "User name is required.")
            .ShouldHaveField(nameof(UserName));
    }

    [Fact(DisplayName = "User name should reject a value over the maximum length when user name is too long")]
    public void Create_Should_ReturnValidationFailure_When_UserNameIsTooLong()
    {
        var result = UserName.Create(new string('a', UserName.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"User name cannot be longer than {UserName.MaxLength} characters.")
            .ShouldHaveField(nameof(UserName));
    }

    [Fact(DisplayName = "User name should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var userName = UserName.Create("tactical-hero").Value;

        var result = userName.ToString();

        result.ShouldBe(userName.Value);
    }
}
