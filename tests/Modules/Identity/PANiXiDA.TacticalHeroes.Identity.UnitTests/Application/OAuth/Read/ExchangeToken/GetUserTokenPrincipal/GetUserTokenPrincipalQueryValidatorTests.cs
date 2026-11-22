using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetUserTokenPrincipal;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.Read.ExchangeToken.GetUserTokenPrincipal;

public sealed class GetUserTokenPrincipalQueryValidatorTests
{
    [Fact(DisplayName = "User token principal validator should accept a valid user id when user id is valid")]
    public void Validate_Should_ReturnValidResult_When_UserIdIsValid()
    {
        var validator = new GetUserTokenPrincipalQueryValidator();

        var result = validator.Validate(new GetUserTokenPrincipalQuery(Guid.CreateVersion7()));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "User token principal validator should reject an empty user id when user id is empty")]
    public void Validate_Should_ReturnError_When_UserIdIsEmpty()
    {
        var validator = new GetUserTokenPrincipalQueryValidator();

        var result = validator.Validate(new GetUserTokenPrincipalQuery(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetUserTokenPrincipalQuery.UserId));
    }
}
