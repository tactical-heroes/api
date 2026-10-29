namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

public sealed partial class MatchmakingRules : ValueObject
{
    private MatchmakingRules(int initialRatingGap, int ratingGapStep, TimeSpan expansionInterval, int maximumRatingGap)
    {
        InitialRatingGap = initialRatingGap;
        RatingGapStep = ratingGapStep;
        ExpansionInterval = expansionInterval;
        MaximumRatingGap = maximumRatingGap;
    }

    public int InitialRatingGap { get; }
    public int RatingGapStep { get; }
    public TimeSpan ExpansionInterval { get; }
    public int MaximumRatingGap { get; }

    public static Result<MatchmakingRules> Create(
        int initialRatingGap,
        int ratingGapStep,
        TimeSpan expansionInterval,
        int maximumRatingGap)
    {
        if (initialRatingGap < 0
            || ratingGapStep <= 0
            || expansionInterval <= TimeSpan.Zero
            || maximumRatingGap < initialRatingGap)
        {
            return Result.Failure<MatchmakingRules>(Error.Validation("Matchmaking rating rules are invalid."));
        }

        return Result.Success(new MatchmakingRules(
            initialRatingGap: initialRatingGap,
            ratingGapStep: ratingGapStep,
            expansionInterval: expansionInterval,
            maximumRatingGap: maximumRatingGap));
    }
}
