using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.Entities.RoleClaims.ValueObjects;

public sealed class ClaimTypeTests
{
    [Fact(DisplayName = "Role claim type should trim a valid value when claim type is valid")]
    public void Create_Should_TrimValue_When_ClaimTypeIsValid()
    {
        var result = ClaimType.Create(" permission ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("permission");
    }

    [Fact(DisplayName = "Role claim type should reject an empty value when claim type is empty")]
    public void Create_Should_ReturnValidationFailure_When_ClaimTypeIsEmpty()
    {
        var result = ClaimType.Create("   ");

        result.ShouldHaveSingleError(ErrorType.Validation, "Claim type cannot be empty.")
            .ShouldHaveField(nameof(ClaimType));
    }

    [Fact(DisplayName = "Role claim type should reject a value over the maximum length when claim type is too long")]
    public void Create_Should_ReturnValidationFailure_When_ClaimTypeIsTooLong()
    {
        var result = ClaimType.Create(new string('a', ClaimType.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Claim type cannot be longer than {ClaimType.MaxLength} characters.")
            .ShouldHaveField(nameof(ClaimType));
    }

    [Fact(DisplayName = "Role claim type should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var claimType = ClaimType.Create("permission").Value;

        var result = claimType.ToString();

        result.ShouldBe(claimType.Value);
    }
}
