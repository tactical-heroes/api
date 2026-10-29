using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.Options.Matchmaking;

public sealed class MatchmakingOptionsValidator : IValidateOptions<MatchmakingOptions>
{
    public ValidateOptionsResult Validate(string? name, MatchmakingOptions options)
    {
        var rules = MatchmakingRules.Create(
            initialRatingGap: options.InitialRatingGap,
            ratingGapStep: options.RatingGapStep,
            expansionInterval: options.ExpansionInterval,
            maximumRatingGap: options.MaximumRatingGap);

        return rules.IsSuccess
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("Matchmaking rating rules are invalid.");
    }
}
