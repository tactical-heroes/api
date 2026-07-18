using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.GetDetails;

public sealed class GetUserDetailsQueryValidatorTests
{
    [Fact(DisplayName = "User details validator should reject an empty user id")]
    public void Validate_Should_ReturnError_When_UserIdIsEmpty()
    {
        var validator = new GetUserDetailsQueryValidator();

        var result = validator.Validate(new GetUserDetailsQuery(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetUserDetailsQuery.Id));
    }
}
