using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Watch;

public sealed class WatchMatchSearchCommandValidatorTests
{
    [Fact(DisplayName = "Validator should accept command when no client selection is required")]
    public void Validate_Should_AcceptCommand_When_NoClientSelectionIsRequired()
    {
        var result = new WatchMatchSearchCommandValidator().Validate(new WatchMatchSearchCommand());

        result.IsValid.ShouldBeTrue();
    }
}
