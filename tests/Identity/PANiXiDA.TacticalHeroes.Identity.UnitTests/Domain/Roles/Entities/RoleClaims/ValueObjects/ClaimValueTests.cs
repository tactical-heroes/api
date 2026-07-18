using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.Entities.RoleClaims.ValueObjects;

public sealed class ClaimValueTests
{
    [Fact(DisplayName = "Role claim value should trim a valid value")]
    public void Create_Should_TrimValue_When_ClaimValueIsValid()
    {
        var result = ClaimValue.Create(" heroes.manage ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("heroes.manage");
    }

    [Fact(DisplayName = "Role claim value should reject an empty value")]
    public void Create_Should_ReturnValidationFailure_When_ClaimValueIsEmpty()
    {
        var result = ClaimValue.Create("   ");

        result.ShouldHaveSingleError(ErrorType.Validation, "Claim value cannot be empty.")
            .ShouldHaveField(nameof(ClaimValue));
    }

    [Fact(DisplayName = "Role claim value should reject a value over the maximum length")]
    public void Create_Should_ReturnValidationFailure_When_ClaimValueIsTooLong()
    {
        var result = ClaimValue.Create(new string('a', ClaimValue.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Claim value cannot be longer than {ClaimValue.MaxLength} characters.")
            .ShouldHaveField(nameof(ClaimValue));
    }
}
