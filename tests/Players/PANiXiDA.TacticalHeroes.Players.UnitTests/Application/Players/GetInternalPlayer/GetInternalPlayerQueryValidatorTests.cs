using PANiXiDA.TacticalHeroes.Players.Application.Players.GetInternalPlayer;

namespace PANiXiDA.TacticalHeroes.Players.UnitTests.Application.Players.GetInternalPlayer;

public sealed class GetInternalPlayerQueryValidatorTests
{
    [Fact(DisplayName = "Validator should accept lookup when user id is valid")]
    public void Validate_Should_AcceptLookup_When_UserIdIsValid()
    {
        var validator = new GetInternalPlayerQueryValidator();

        var result = validator.Validate(new GetInternalPlayerQuery(Guid.CreateVersion7()));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Validator should reject lookup when user id is empty")]
    public void Validate_Should_RejectLookup_When_UserIdIsEmpty()
    {
        var validator = new GetInternalPlayerQueryValidator();

        var result = validator.Validate(new GetInternalPlayerQuery(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetInternalPlayerQuery.UserId));
    }
}
