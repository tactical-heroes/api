using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.Options.MatchmakingProcessing;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Scheduling.Options.MatchmakingProcessing;

public sealed class MatchmakingProcessingOptionsValidatorTests
{
    [Theory(DisplayName = "Validator should validate timer interval when options are supplied")]
    [InlineData(1000, true)]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(4294967295L, false)]
    public void Validate_Should_ValidateTimerInterval_When_OptionsAreSupplied(long milliseconds, bool valid)
    {
        var validator = new MatchmakingProcessingOptionsValidator();
        var options = new MatchmakingProcessingOptions { Interval = TimeSpan.FromMilliseconds(milliseconds) };

        var result = validator.Validate(null, options);

        result.Succeeded.ShouldBe(valid);
    }
}
