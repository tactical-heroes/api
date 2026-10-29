namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.Options.Matchmaking;

public sealed class MatchmakingOptions
{
    public const string SectionName = $"{nameof(Matchmaking)}:Matching";

    public int InitialRatingGap { get; init; } = 100;
    public int RatingGapStep { get; init; } = 50;
    public TimeSpan ExpansionInterval { get; init; } = TimeSpan.FromSeconds(15);
    public int MaximumRatingGap { get; init; } = 500;
}
