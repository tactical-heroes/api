using PANiXiDA.TacticalHeroes.Identity.Application.Auth.GetAuthenticatedUser;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.GetAuthenticatedUser;

public sealed class GetAuthenticatedUserQueryValidatorTests
{
    [Theory(DisplayName = "Authenticated user validator should validate the identifier when user id is checked")]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_Should_ValidateIdentifier_When_UserIdIsChecked(bool valid)
    {
        var validator = new GetAuthenticatedUserQueryValidator();

        var result = validator.Validate(new GetAuthenticatedUserQuery(
            valid ? Guid.CreateVersion7() : Guid.Empty, "stamp"));

        result.IsValid.ShouldBe(valid);
    }
}
