using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users.Entities.UserClaims.ValueObjects;

public sealed class ClaimValueTests
{
    [Fact(DisplayName = "User claim value should trim a valid value when claim value is valid")]
    public void Create_Should_TrimValue_When_ClaimValueIsValid()
    {
        var result = ClaimValue.Create(" heroes.read ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("heroes.read");
        result.Value.ToString().ShouldBe("heroes.read");
    }

    [Theory(DisplayName = "User claim value should reject an empty value when claim value is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_ClaimValueIsEmpty(string value)
    {
        var result = ClaimValue.Create(value);

        result.ShouldHaveSingleError(ErrorType.Validation, "Claim value cannot be empty.")
            .ShouldHaveField(nameof(ClaimValue));
    }

    [Fact(DisplayName = "User claim value should reject a value over the maximum length when claim value is too long")]
    public void Create_Should_ReturnValidationFailure_When_ClaimValueIsTooLong()
    {
        var result = ClaimValue.Create(new string('a', ClaimValue.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Claim value cannot be longer than {ClaimValue.MaxLength} characters.")
            .ShouldHaveField(nameof(ClaimValue));
    }

    [Fact(DisplayName = "User claim value should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var claimValue = ClaimValue.Create("heroes.read").Value;

        var result = claimValue.ToString();

        result.ShouldBe(claimValue.Value);
    }
}
