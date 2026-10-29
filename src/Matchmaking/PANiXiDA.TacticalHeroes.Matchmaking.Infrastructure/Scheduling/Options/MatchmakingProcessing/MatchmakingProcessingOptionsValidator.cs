using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.Options.MatchmakingProcessing;

public sealed class MatchmakingProcessingOptionsValidator : IValidateOptions<MatchmakingProcessingOptions>
{
    public ValidateOptionsResult Validate(string? name, MatchmakingProcessingOptions options)
    {
        return options.Interval < TimeSpan.FromMilliseconds(1)
            || options.Interval > TimeSpan.FromMilliseconds(uint.MaxValue - 1)
                ? ValidateOptionsResult.Fail("Matchmaking interval must be supported by PeriodicTimer.")
                : ValidateOptionsResult.Success;
    }
}
