using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Process;

public sealed class ProcessMatchmakingCommandValidatorTests
{
    [Fact(DisplayName = "Validator should accept scheduled tick when command is created")]
    public void Validate_Should_AcceptScheduledTick_When_CommandIsCreated()
    {
        var validator = new ProcessMatchmakingCommandValidator();

        var result = validator.Validate(new ProcessMatchmakingCommand());

        result.IsValid.ShouldBeTrue();
    }
}
