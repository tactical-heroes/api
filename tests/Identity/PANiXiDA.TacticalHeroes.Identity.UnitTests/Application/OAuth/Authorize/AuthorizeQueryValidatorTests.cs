using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Authorize;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.Authorize;

public sealed class AuthorizeQueryValidatorTests
{
    [Fact(DisplayName = "Validate should return valid result when user id is valid")]
    public void Validate_Should_ReturnValidResult_When_UserIdIsValid()
    {
        var validator = new AuthorizeQueryValidator();

        var result = validator.Validate(new AuthorizeQuery(Guid.CreateVersion7()));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Validate should return error when user id is empty")]
    public void Validate_Should_ReturnError_When_UserIdIsEmpty()
    {
        var validator = new AuthorizeQueryValidator();

        var result = validator.Validate(new AuthorizeQuery(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(AuthorizeQuery.UserId));
    }
}
