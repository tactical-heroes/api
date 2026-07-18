using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetClientTokenPrincipal;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.GetClientTokenPrincipal;

public sealed class GetClientTokenPrincipalQueryValidatorTests
{
    [Fact(DisplayName = "Client token principal validator should accept a client id")]
    public void Validate_Should_ReturnValidResult_When_ClientIdIsProvided()
    {
        var validator = new GetClientTokenPrincipalQueryValidator();

        var result = validator.Validate(
            new GetClientTokenPrincipalQuery("tactical-heroes-service"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Client token principal validator should reject an empty client id")]
    public void Validate_Should_ReturnError_When_ClientIdIsEmpty()
    {
        var validator = new GetClientTokenPrincipalQueryValidator();

        var result = validator.Validate(new GetClientTokenPrincipalQuery(""));

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(GetClientTokenPrincipalQuery.ClientId));
    }
}
