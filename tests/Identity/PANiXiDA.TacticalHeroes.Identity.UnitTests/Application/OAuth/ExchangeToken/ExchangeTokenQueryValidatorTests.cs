using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.ExchangeToken;

public sealed class ExchangeTokenQueryValidatorTests
{
    [Fact(DisplayName = "Exchange token validator should accept a valid user id")]
    public void Validate_Should_ReturnValidResult_When_UserIdIsValid()
    {
        var validator = new ExchangeTokenQueryValidator();

        var result = validator.Validate(new ExchangeTokenQuery(Guid.CreateVersion7()));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Exchange token validator should reject an empty user id")]
    public void Validate_Should_ReturnError_When_UserIdIsEmpty()
    {
        var validator = new ExchangeTokenQueryValidator();

        var result = validator.Validate(new ExchangeTokenQuery(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ExchangeTokenQuery.UserId));
    }
}
