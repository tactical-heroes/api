using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Expire;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Expire;

public sealed class ExpireMatchSearchesCommandValidatorTests
{
    [Fact(DisplayName = "Validator should accept expiration tick when command is created")]
    public void Validate_Should_AcceptExpirationTick_When_CommandIsCreated()
    {
        var validator = new ExpireMatchSearchesCommandValidator();

        var result = validator.Validate(new ExpireMatchSearchesCommand());

        result.IsValid.ShouldBeTrue();
    }
}
