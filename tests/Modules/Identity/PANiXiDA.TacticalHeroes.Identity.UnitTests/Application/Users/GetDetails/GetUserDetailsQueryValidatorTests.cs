using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetDetails;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.UnitTests.Application.Users.GetDetails;

public sealed class GetUserDetailsQueryValidatorTests
{
    [Fact(DisplayName = "User details validator should reject an empty user id when user id is empty")]
    public void Validate_Should_ReturnError_When_UserIdIsEmpty()
    {
        var validator = new GetUserDetailsQueryValidator();

        var result = validator.Validate(new GetUserDetailsQuery(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetUserDetailsQuery.Id));
    }
}
