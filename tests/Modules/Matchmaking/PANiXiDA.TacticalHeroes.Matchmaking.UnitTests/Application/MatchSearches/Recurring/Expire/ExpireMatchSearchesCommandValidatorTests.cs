using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Recurring.Expire;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Recurring.Expire;

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
