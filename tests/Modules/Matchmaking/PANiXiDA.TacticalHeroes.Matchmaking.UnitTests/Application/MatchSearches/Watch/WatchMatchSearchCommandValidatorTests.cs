using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Watch;

public sealed class WatchMatchSearchCommandValidatorTests
{
    [Theory(DisplayName = "Validator should require connection id when subscribing")]
    [InlineData("", false)]
    [InlineData("connection", true)]
    public void Validate_Should_RequireConnectionId_When_Subscribing(string connectionId, bool valid)
    {
        var result = new WatchMatchSearchCommandValidator().Validate(new WatchMatchSearchCommand(connectionId));

        result.IsValid.ShouldBe(valid);
    }
}
