using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.GetUserInfo;

public sealed class GetUserInfoQueryValidatorTests
{
    [Fact(DisplayName = "User info validator should accept a valid user id")]
    public void Validate_Should_ReturnValidResult_When_UserIdIsValid()
    {
        var validator = new GetUserInfoQueryValidator();

        var result = validator.Validate(new GetUserInfoQuery(Guid.CreateVersion7()));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "User info validator should reject an empty user id")]
    public void Validate_Should_ReturnError_When_UserIdIsEmpty()
    {
        var validator = new GetUserInfoQueryValidator();

        var result = validator.Validate(new GetUserInfoQuery(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetUserInfoQuery.UserId));
    }
}
