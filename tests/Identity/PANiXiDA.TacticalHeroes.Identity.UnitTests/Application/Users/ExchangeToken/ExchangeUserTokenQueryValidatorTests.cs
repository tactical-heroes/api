using PANiXiDA.TacticalHeroes.Identity.Application.Users.ExchangeToken;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.ExchangeToken;

public sealed class ExchangeUserTokenQueryValidatorTests
{
    [Fact(DisplayName = "Exchange token validator should accept a valid user id when user id is valid")]
    public void Validate_Should_ReturnValidResult_When_UserIdIsValid()
    {
        var validator = new ExchangeUserTokenQueryValidator();

        var result = validator.Validate(new ExchangeUserTokenQuery(Guid.CreateVersion7()));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Exchange token validator should reject an empty user id when user id is empty")]
    public void Validate_Should_ReturnError_When_UserIdIsEmpty()
    {
        var validator = new ExchangeUserTokenQueryValidator();

        var result = validator.Validate(new ExchangeUserTokenQuery(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ExchangeUserTokenQuery.UserId));
    }
}
