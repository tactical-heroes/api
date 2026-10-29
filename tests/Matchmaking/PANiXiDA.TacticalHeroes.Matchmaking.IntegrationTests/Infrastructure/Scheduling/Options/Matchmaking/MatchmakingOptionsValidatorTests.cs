using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.Options.Matchmaking;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Scheduling.Options.Matchmaking;

public sealed class MatchmakingOptionsValidatorTests
{
    [Theory(DisplayName = "Validator should validate matching configuration when options are supplied")]
    [InlineData(100, 50, 15, 500, true)]
    [InlineData(-1, 50, 15, 500, false)]
    [InlineData(100, 0, 15, 500, false)]
    [InlineData(100, 50, 0, 500, false)]
    [InlineData(100, 50, 15, 99, false)]
    public void Validate_Should_ValidateMatchingConfiguration_When_OptionsAreSupplied(
        int initial, int step, int seconds, int maximum, bool valid)
    {
        var options = new MatchmakingOptions
        {
            InitialRatingGap = initial,
            RatingGapStep = step,
            ExpansionInterval = TimeSpan.FromSeconds(seconds),
            MaximumRatingGap = maximum
        };
        var validator = new MatchmakingOptionsValidator();

        var result = validator.Validate(null, options);

        result.Succeeded.ShouldBe(valid);
    }
}
